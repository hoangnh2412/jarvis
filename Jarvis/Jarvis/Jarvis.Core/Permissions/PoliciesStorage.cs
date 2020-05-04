using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Reflection;
using Infrastructure;
using Infrastructure.Abstractions;
using Jarvis.Core.Constants;

namespace Jarvis.Core.Permissions
{
    public interface IPoliciesStorage
    {
        List<PolicyModel> GetPolicies();
    }

    public class PoliciesStorage : IPoliciesStorage
    {
        private List<PolicyModel> _policies = null;
        private readonly IModuleManager _moduleManager;

        public PoliciesStorage(IModuleManager moduleManager)
        {
            _moduleManager = moduleManager;
        }

        public List<PolicyModel> GetPolicies()
        {
            if (_policies != null)
                return _policies;

            _policies = new List<PolicyModel>();
            var types = _moduleManager.GetImplementations<IPolicy>();
            foreach (var type in types)
            {
                var fields = type.GetFields();
                foreach (var item in fields)
                {
                    var key = item.Name;
                    var data = item.GetValue(item) as Tuple<string, List<ClaimOfResource>, List<ClaimOfChildResource>>;
                    if (_policies.Any(x => x.Code == key))
                        throw new Exception($"Policy info Key: {key} - Name: {data.Item1} has ben exist");

                    var policy = new PolicyModel
                    {
                        Code = key,
                        Name = data.Item1,
                        ClaimOfResource = data.Item2,
                        ClaimOfChildResources = data.Item3
                    };

                    var attrGroup = item.DeclaringType.GetCustomAttribute(typeof(DisplayAttribute)) as DisplayAttribute;
                    if (attrGroup != null)
                    {
                        policy.GroupCode = item.DeclaringType.Name;
                        policy.GroupName = attrGroup.Name;
                        policy.GroupOrder = attrGroup.GetOrder().HasValue ? attrGroup.GetOrder().Value : 0;
                    }

                    if (item.DeclaringType.DeclaringType != null)
                    {
                        var attrModule = item.DeclaringType.DeclaringType.GetCustomAttribute(typeof(DisplayAttribute)) as DisplayAttribute;
                        if (attrModule != null)
                        {
                            policy.ModuleCode = item.DeclaringType.DeclaringType.Name;
                            policy.ModuleName = attrModule.Name;
                            policy.ModuleOrder = attrModule.GetOrder().HasValue ? attrModule.GetOrder().Value : 0;
                        }
                    }

                    _policies.Add(policy);
                }
            }

            _policies = _policies.OrderBy(x => x.ModuleOrder).ThenBy(x => x.GroupOrder).ToList();
            return _policies;
        }
    }
}
