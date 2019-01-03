using System.ComponentModel;

namespace KPreisser.UI
{
    /// <summary>
    /// 
    /// </summary>
    [TypeConverter(typeof(ExpandableObjectConverter))]
    public struct TaskDialogProgressBarRange
    {
        private int minimum;

        private int maximum;


        /// <summary>
        /// 
        /// </summary>
        /// <param name="minimum"></param>
        /// <param name="maximum"></param>
        public TaskDialogProgressBarRange(int minimum, int maximum)
        {
            this.minimum = minimum;
            this.maximum = maximum;
        }


        /// <summary>
        /// 
        /// </summary>
        /// <param name="tuple"></param>
        public static implicit operator TaskDialogProgressBarRange(
                (int minimum, int maximum) tuple)
        {
            return new TaskDialogProgressBarRange(tuple.minimum, tuple.maximum);
        }


        /// <summary>
        /// 
        /// </summary>
        public int Minimum
        {
            get => this.minimum;
            set => this.minimum = value;
        }

        /// <summary>
        /// 
        /// </summary>
        public int Maximum
        {
            get => this.maximum;
            set => this.maximum = value;
        }


        /// <summary>
        /// 
        /// </summary>
        /// <param name="minimum"></param>
        /// <param name="maximum"></param>
        public void Deconstruct(out int minimum, out int maximum)
        {
            minimum = this.minimum;
            maximum = this.maximum;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <returns></returns>
        public override string ToString()
        {
            return (this.minimum, this.maximum).ToString();
        }
    }
}
