namespace Dbarone.Net.Mine;

public class ItemSet
{
    public ItemSet(IEnumerable<object> items)
    {
        Values = items;
    }

    public IEnumerable<object> Values { get; set; }

    public int Size
    {
        get { return Values.Count(); }
    }

    public ItemSet Join(ItemSet b)
    {
        if (this.Size != b.Size)
            throw new ApplicationException("Itemsets being joined must be of same size");

        // if (size-1) values are same, then can join. Otherwise cannot
        if (this.Values.Intersect(b.Values).Count() != this.Size - 1)
            return null;
        else
            return new ItemSet(this.Values.Union(b.Values));
    }

    public override string ToString()
    {
        return string.Join("->", Values.OrderBy(s => s).ToArray());
    }

    public override int GetHashCode()
    {
        return ToString().GetHashCode();
    }

    public override bool Equals(object obj)
    {
        return this.GetHashCode().Equals(obj.GetHashCode());
    }

    /// <summary>
    /// Returns true if the superset specified contains all elements in the current ItemSet.
    /// </summary>
    /// <param name="superSet"></param>
    /// <returns></returns>
    public bool IsSubsetOf(ItemSet superSet)
    {
        return (superSet.Values.Intersect(this.Values).Count() == this.Values.Count() && this.Size < superSet.Size);
    }
}
