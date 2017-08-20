using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Aurora.Utils
{
    public class Tree<T>
    {
        private T _Item;

        /// <summary>
        /// The tree item
        /// </summary>
        public T Item { get { return _Item; } }

        private HashSet<Tree<T>> _Children = new HashSet<Tree<T>>();

        public Tree(T rootNode)
        {
            _Item = rootNode;
        }

        public Tree(T[] items, int StartIndex = 0)
        {
            //First item is the rootNode
            if (StartIndex < items.Length)
                _Item = items[StartIndex];

            if (StartIndex + 1 < items.Length)
                _Children.Add(new Tree<T>(items, StartIndex + 1));
        }

        public Tree<T> AddBranch(T[] items, int StartIndex = 0)
        {
            if (StartIndex < items.Length)
            {
                Tree<T> _equalChild = ContainsItem(items[StartIndex]);

                if (_equalChild == null)
                {
                    _Children.Add(new Tree<T>(items[StartIndex]).AddBranch(items, StartIndex + 1));
                }
                else
                {
                    if (StartIndex + 1 < items.Length)
                        _equalChild.AddBranch(items, StartIndex + 1);
                }
            }

            return this;
        }

        public Tree<T> ContainsItem(T item)
        {
            foreach (var child in _Children)
            {
                if (child._Item.Equals(item))
                    return child;
            }

            return null;
        }

        public T[] GetChildren()
        {
            List<T> _returnChildren = new List<T>();

            foreach (var child in _Children)
                _returnChildren.Add(child._Item);

            return _returnChildren.ToArray();
        }

        public T[] GetAllChildren()
        {
            List<T> _returnChildren = new List<T>();

            foreach (var child in _Children)
            {
                _returnChildren.Add(child._Item);
                T[] _childChildren = child.GetAllChildren();
                _returnChildren.AddRange(_childChildren);
            }
                
            return _returnChildren.ToArray();
        }

        public override bool Equals(object obj)
        {
            if (ReferenceEquals(null, obj)) return false;
            if (ReferenceEquals(this, obj)) return true;
            if (obj.GetType() != this.GetType()) return false;
            return Equals((Tree<T>)obj);
        }

        public bool Equals(Tree<T> p)
        {
            if (ReferenceEquals(null, p)) return false;
            if (ReferenceEquals(this, p)) return true;

            bool _childrenEqual = false;

            if(_Children.Count == p._Children.Count)
            {
                foreach(var child in _Children)
                {
                    Tree<T> pTree = p.ContainsItem(child._Item);

                    if(!child.Equals(pTree))
                    {
                        _childrenEqual = false;
                        break;
                    }
                }
            }

            return (_Item.Equals(p._Item)) &&
                _childrenEqual;
        }

        public override int GetHashCode()
        {
            unchecked
            {
                int hash = 17;
                hash = hash * 23 + _Item.GetHashCode();
                hash = hash * 23 + _Children.GetHashCode();
                return hash;
            }
        }
    }
}
