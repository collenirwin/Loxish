namespace Lang.Interpreter
{
    /// <summary>
    /// A lang instance of a <see cref="Class"/>.
    /// </summary>
    public class Instance
    {
        private readonly Class _class;

        /// <summary>
        /// Initialize an <see cref="Instance"/> with the <see cref="Class"/> it is an instance of.
        /// </summary>
        /// <param name="class"><see cref="Class"/> this is an instance of.</param>
        public Instance(Class @class)
        {
            _class = @class;
        }

        public override string ToString()
        {
            return $"{_class.Name} instance";
        }
    }
}
