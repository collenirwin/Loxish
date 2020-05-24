using System.Collections.Generic;
using System.Linq;

namespace Lang.Interpreter
{
    /// <summary>
    /// A callable lang function.
    /// </summary>
    public class Function : ICallable
    {
        private readonly FunctionStatement _declaration;

        /// <summary>
        /// The number of arguments this function requires.
        /// </summary>
        public int ArgumentCount { get; }

        /// <summary>
        /// Initialize a <see cref="Function"/> with the
        /// <see cref="FunctionStatement"/> declaration it originated from.
        /// </summary>
        /// <param name="declaration">Parsed source of the function.</param>
        public Function(FunctionStatement declaration)
        {
            _declaration = declaration;
            ArgumentCount = _declaration.Params.Count();
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
            var environment = new EnvironmentState(outerEnvironment: interpreter.GlobalState);

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
                return ex.Value;
            }
            
            return null;
        }

        public override string ToString()
        {
            return $"<fun {_declaration.Name.WrappedSource}>";
        }
    }
}
