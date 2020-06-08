using System.Collections.Generic;
using System.Linq;

namespace Lang.Interpreter
{
    /// <summary>
    /// A callable lang function.
    /// </summary>
    public class Function : ICallable
    {
        private readonly Token _name;
        private readonly FunctionExpression _declaration;
        private readonly EnvironmentState _closure;
        private readonly bool _isInit;

        /// <summary>
        /// The number of arguments this function requires.
        /// </summary>
        public int ParamCount { get; }

        /// <summary>
        /// Initialize a <see cref="Function"/> with the
        /// <see cref="FunctionStatement"/> declaration it originated from.
        /// </summary>
        /// <param name="declaration">Parsed source of the function.</param>
        public Function(FunctionExpression declaration, EnvironmentState closure,
            Token name = null, bool isInit = false)
        {
            _declaration = declaration;
            _closure = closure;
            _name = name;
            _isInit = isInit;
            ParamCount = _declaration.Params.Count();
        }

        /// <summary>
        /// Calls the function.
        /// </summary>
        /// <param name="interpreter">Interpreter calling the function.</param>
        /// <param name="arguments">Arguments to pass to the function.</param>
        /// <returns>The result of the function call.</returns>
        public object Call(Interpreter interpreter, IEnumerable<object> arguments)
        {
            // each call gets its own scope so that it can keep track of its individual state
            var environment = new EnvironmentState(outerEnvironment: _closure);

            // combine the params and the arguments into one collection
            var paramArguments = _declaration.Params.Zip(arguments, (param, argument) => new
            {
                Name = param.WrappedSource,
                Value = argument
            });

            // define scoped variables for each param/argument
            foreach (var item in paramArguments)
            {
                environment.Define(item.Name, item.Value);
            }

            try
            {
                // execute the function body with the new scope we just created
                interpreter.ExecuteBlock(_declaration.Body, environment);
            }
            catch (ReturnException ex)
            {
                return _isInit
                    ? _closure.GetValue("this", 0)
                    : ex.Value;
            }
            
            // constructors will always return 'this'
            return _isInit
                ? _closure.GetValue("this", 0)
                : null;
        }

        /// <summary>
        /// Binds this function to an instance of a class,
        /// allowing it to access the instance's 'this' variable.
        /// </summary>
        /// <param name="instance">Instance to bind to.</param>
        /// <returns>The bound function.</returns>
        public Function Bind(Instance instance)
        {
            var environment = new EnvironmentState(outerEnvironment: _closure);
            environment.Define("this", instance);
            return new Function(_declaration, environment, _name, _isInit);
        }

        public override string ToString() => $"<fun {_name?.WrappedSource ?? "<anonymous>"}>";
    }
}
