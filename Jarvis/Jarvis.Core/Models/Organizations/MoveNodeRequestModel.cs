using System;

namespace Jarvis.Core.Models.Organizations
{
    public class MoveNodeRequestModel
    {
        public Guid? ParentNode { get; set; }
        public Guid? LeftNode { get; set; }
        public Guid? RightNode { get; set; }
    }
}