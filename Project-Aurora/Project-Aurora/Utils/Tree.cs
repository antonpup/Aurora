using System.Collections.Generic;

namespace Aurora.Utils;

public class Tree<T> where T : notnull
{
    private readonly T _item;

    /// <summary>
    /// The tree item
    /// </summary>
    public T Item => _item;

    public bool IsLeaf => _children.Count == 0;

    private readonly HashSet<Tree<T>> _children = new();

    public Tree(T rootNode)
    {
        _item = rootNode;
    }

    public Tree<T> AddBranch(T[] items, int startIndex = 0)
    {
        if (startIndex < items.Length)
        {
            var equalChild = ContainsItem(items[startIndex]);

            if (equalChild == null)
            {
                _children.Add(new Tree<T>(items[startIndex]).AddBranch(items, startIndex + 1));
            }
            else
            {
                if (startIndex + 1 < items.Length)
                    equalChild.AddBranch(items, startIndex + 1);
            }
        }

        return this;
    }

    public Tree<T>? ContainsItem(T item)
    {
        foreach (var child in _children)
        {
            if (child._item.Equals(item))
                return child;
        }

        return null;
    }

    public T[] GetChildren()
    {
        var returnChildren = new List<T>();

        foreach (var child in _children)
            returnChildren.Add(child._item);

        return returnChildren.ToArray();
    }

    public T[] GetAllChildren()
    {
        var returnChildren = new List<T>();

        foreach (var child in _children)
        {
            returnChildren.Add(child._item);
            var childChildren = child.GetAllChildren();
            returnChildren.AddRange(childChildren);
        }
                
        return returnChildren.ToArray();
    }

    public Tree<T>? GetNodeByPath(IEnumerable<T> path)
    {
        var node = this;
        foreach (var key in path)
        {
            node = node?.ContainsItem(key);
        }
        return node;
    }

    public override bool Equals(object? obj)
    {
        if (ReferenceEquals(null, obj)) return false;
        if (ReferenceEquals(this, obj)) return true;
        if (obj.GetType() != GetType()) return false;
        return Equals((Tree<T>)obj);
    }

    public bool Equals(Tree<T>? p)
    {
        if (ReferenceEquals(null, p)) return false;
        if (ReferenceEquals(this, p)) return true;

        var childrenEqual = false;

        if(_children.Count == p._children.Count)
        {
            foreach(var child in _children)
            {
                var pTree = p.ContainsItem(child._item);

                if(!child.Equals(pTree))
                {
                    childrenEqual = false;
                    break;
                }
            }
        }

        return _item.Equals(p._item) && childrenEqual;
    }

    public override int GetHashCode()
    {
        unchecked
        {
            var hash = 17;
            hash = hash * 23 + _item.GetHashCode();
            hash = hash * 23 + _children.GetHashCode();
            return hash;
        }
    }
}