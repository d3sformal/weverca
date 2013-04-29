using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Weverca.ControlFlowGraph.MemoryModel
{
    class Table 
    {
        public Dictionary<string, Variable> data;
        Variable unknown;
        public Table()
        {
            data = new Dictionary<string, Variable>();
            unknown = null;
        }
    }

    class Variable 
    {
        List<Value> values;
        Dictionary<string, Variable> mayAliases;
        Dictionary<string, Variable> mustAliases;
        //dva next level 1 pre objekty 1 pre polia???
        Table nextLevel;
        bool isUnknown;
        public Variable() 
        {
            isUnknown = false;
            nextLevel = new Table();
            mayAliases = new Dictionary<string, Variable>();
            mustAliases = new Dictionary<string, Variable>();
            values = new List<Value>();
        }
    }

    abstract class Value 
    { 
    
    }

    class IntegerValue : Value 
    {
        public int value;
    }

    class StringValue : Value 
    {
        public string value;
    
    }

    class BooleanValue : Value
    {
        public bool value;
    }

    class FloatValue : Value
    {
        public double value;
    }

    class UndefinedValue : Value
    {
    }

    class MemoryModel : ICloneable
    {
        private Table table;
        public MemoryModel()
        {
            table = new Table();
        }

        public void Merge(MemoryModel m) 
        {
        
        }

        //set value
        //ako resprezentovat $a["a"]["a"]["a"] 4 stringy?

        //set value to unknown

        //get value

        // remove value??
        public Object Clone()
        {
            throw new NotImplementedException();
        }
     }
}
