using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EliteCMD
{
    class cargo
    {
        public Dictionary<string, int> inventory = new Dictionary<string, int>();
        public Dictionary<string, string> dictionary = new Dictionary<string, string>();

        public cargo()
        {

        }

        public int AddComodity(string Name, string LocaliszedName, int Amount)
        {
            if(!dictionary.ContainsKey(Name))
            {
                dictionary.Add(Name, LocaliszedName);
            }

            if(inventory.ContainsKey(Name))
            {
                inventory[Name] += Amount;
            }
            else
            {
                inventory.Add(Name, Amount);
            }
            return Count();
        }

        public int RemoveComodity(string Name, int Amount)
        {
            if(inventory.ContainsKey(Name))
            {
                if(inventory[Name] > Amount)
                {
                    inventory[Name] -= Amount;
                }
                else
                {
                    inventory.Remove(Name);
                }
            }
            return Count();
        }

        public int Count()
        {
            return inventory.Sum(s => s.Value); ;
        }

    }
}
