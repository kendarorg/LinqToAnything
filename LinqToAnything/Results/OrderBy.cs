namespace LinqToAnything.Results
{
    public class OrderBy
    {
        public OrderBy(string name, OrderByDirection direction)
        {
            Name = name;
            Direction = direction;
        }

        public enum OrderByDirection
        {
            Asc,
            Desc
        }

        public string Name { get; set; }
        public OrderByDirection Direction { get; set; }

        public OrderBy Clone()
        {
            return new OrderBy(this.Name, this.Direction);
        }

        public override string ToString()
        {
            return  Name + " " + this.Direction.ToString().ToUpper();
        }
    }
}