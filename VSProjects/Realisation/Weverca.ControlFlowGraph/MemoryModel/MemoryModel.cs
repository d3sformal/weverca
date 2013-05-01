using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Weverca.ControlFlowGraph.MemoryModel
{
    class Table 
    {
        Dictionary<string, Variable> data;
        public Variable unknown;

        public Table()
        {
            data = new Dictionary<string, Variable>();
            unknown = null;
        }

        public Variable GetOrCreateVariable(string variableName)
        {
            Variable res;
            if (!data.TryGetValue(variableName, out res))
            {
                res = new Variable();
                data[variableName] = res;
            }
            return res;
        }

        public bool TryGetVariable(string variableName,out Variable res) 
        {
            return data.TryGetValue(variableName, out res);
        }

        public void Remove(string variableName)
        {
            data.Remove(variableName);
        }

        public Variable this[string variableName]
        {
            get
            {
                return GetOrCreateVariable(variableName);
            }
        }


        internal void Clear()
        {
            throw new NotImplementedException();
        }
    }

    class Variable
    {
        public readonly List<Value> values;
        public readonly HashSet<Variable> mayAliases;
        public readonly HashSet<Variable> mustAliases;
        public readonly Table nextLevel;
        public readonly Variable previousLevel;

        bool isAnyValue;
        public Variable() 
        {
            isAnyValue = false;
            nextLevel = new Table();
            mayAliases = new HashSet<Variable>();
            mustAliases = new HashSet<Variable>();
            values = new List<Value>();
        }

        internal void SetValue(Value value)
        {
            setValue(value);

            foreach (Variable must in mustAliases)
            {
                must.setValue(value);
            }
        }

        private void setValue(Value value)
        {
            values.Clear();
            nextLevel.Clear();
            values.Add(value);

            foreach (Variable may in mayAliases)
            {
                may.addValue(value);
            }
        }

        private void addValue(Value value)
        {
            values.Add(value);

            /*

Test chování aliasů - při vytváření může vzniknout cyklus!!!!
***********************PHP
<?

$v = array(1, 2, 3);
$v[1] = array(4, 5, 6);

echo "$v[0] $v[1] $v[2]<br>";
echo $v[1][0]. " ".$v[1][1]. " ".$v[1][2]."<br>";

if (!$_GET['t'])
{
    $v[1] = &$v;
}

echo "$v[0] $v[1] $v[2]<br>";
echo $v[1][0]. " ".$v[1][1]. " ".$v[1][2]."<br>";
?>

<a href="alias.php?t=<?=!$_GET['t']?>" > test <?=$_GET['t']?></a>

***********************PHP
             
             */
        }
    }

    class ObjectReference : Value
    {
        Table table;
        ClassDeclaration classInfo;
        HashSet<Variable> referenced;
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

        public Variable GetVariable(string variableName)
        {
            return table.GetOrCreateVariable(variableName);
        }

        public void SetVariableValue(string variableName, Value value)
        {
            Variable variable =  table.GetOrCreateVariable(variableName);
            variable.SetValue(value);
            
        }
        public Variable this[string variableName]
        {
            get
            {
                return table.GetOrCreateVariable(variableName);
            }
        }

        //set value
        //ako resprezentovat $a["a"]["a"]["a"] 4 stringy?
        //$a["a"]->a["a"]
        //set value to unknown

        //get value

        // remove value??
        public Object Clone()
        {
            return Copy();
        }

        public MemoryModel Copy()
        {
            throw new NotImplementedException();
        }
     }
}
