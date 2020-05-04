using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Infrastructure.Abstractions.ChainOfResponsibilityPattern
{
    public abstract class Handler<TInput, TOutput> : IHandler<TInput, TOutput>
    {
        private readonly List<IHandler<TInput, TOutput>> _handlers;
        private Func<Func<TInput, TOutput>, Func<TInput, TOutput>> _chainedDelegate;

        protected Handler()
        {
            _handlers = new List<IHandler<TInput, TOutput>>();
        }

        private Func<Func<TInput, TOutput>, Func<TInput, TOutput>> ChainedDelegate
        {
            get
            {
                if (_chainedDelegate != null)
                {
                    return _chainedDelegate;
                }

                Func<Func<TInput, TOutput>, Func<TInput, TOutput>> chainedDelegate = next => input => _handlers.Last().Handle(input, next);

                for (var index = _handlers.Count - 2; index >= 0; index--)
                {
                    var handler = _handlers[index];
                    var chainedDelegateCloned = chainedDelegate;

                    chainedDelegate = next => input => handler.Handle(input, chainedDelegateCloned(next));
                }

                return _chainedDelegate = chainedDelegate;
            }
        }

        /// <summary>
        /// <para>Invokes handlers one by one until the input has been processed by a handler and returns output, ignoring the rest of the handlers.</para>
        /// <para>It is done by first creaTInputg a pipeline execution delegate from exisTInputg handlers then invoking that delegate against the input.</para>
        /// </summary>
        /// <param name="input">The input object.</param>
        /// <param name="next">The next handler in the chain. If null is provided, <see cref="ThrowNotSupported{TInput,TOutput}"/> will be set as the end of the chain.</param>
        /// <returns></returns>
        /// <exception cref="ArgumentNullException">Thrown if handler list is null.</exception>
        /// <exception cref="ArgumentException">Thrown if handler list is empty.</exception>
        public virtual TOutput Handle(TInput input, Func<TInput, TOutput> next)
        {
            if (next == null)
            {
                Func<TInput, TOutput> func = _ =>
                {
                    throw new NotSupportedException($"Cannot handle this input. Input information: {typeof(TInput)}");
                };
                next = func;
            }

            return ChainedDelegate.Invoke(next).Invoke(input);
        }

        /// <summary>
        /// <para>Performs interception to the given <paramref name="handler"/> object.</para>
        /// <para>Then adds the intercepted handler to the last position in the chain.</para>
        /// </summary>
        /// <param name="handler">The handler object.</param>
        protected void AddHandler<THandler>(THandler handler) where THandler : class, IHandler<TInput, TOutput>
        {
            _handlers.Add(handler);
        }
    }
}