using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Weverca.MemoryModel.Deprecated
{
    class Variable
    {
        public List<Value> values;
        public HashSet<Variable> mayAliases;
        public HashSet<Variable> mustAliases;
        public Table nextLevel;

        Variable mustRepresentant = null;



        public bool HasMayAliases { get { return mayAliases.Count > 0; } }

        public bool HasMustAliases { get { return mustAliases.Count > 0; } }

        public bool IsRepresentant { get { return mustRepresentant == this; } }

        public Variable()
        {
            reset();
        }

        private void reset()
        {
            nextLevel = new Table();
            mayAliases = new HashSet<Variable>();
            mustAliases = new HashSet<Variable>();
            values = new List<Value>();

            mustRepresentant = this;
        }

        public void SetAlias(Variable alias)
        {
            //Alias na sebe sama
            if (alias == this)
            {
                return;
            }

            UnsetVariable();

            values = alias.values;
            nextLevel = alias.nextLevel;
            mayAliases = alias.mayAliases;
            mustRepresentant = alias;

            foreach (Variable subAlias in alias.mustAliases)
            {
                mustAliases.Add(subAlias);
                subAlias.mustAliases.Add(this);
            }

            mustAliases.Add(alias);
            alias.mustAliases.Add(this);
        }

        internal void setResolvedAlias(Variable alias)
        {
            values = alias.values;
            nextLevel = alias.nextLevel;
            mayAliases = alias.mayAliases;
            mustRepresentant = alias;

            foreach (Variable subAlias in alias.mustAliases)
            {
                mustAliases.Add(subAlias);
                subAlias.mustAliases.Add(this);
            }

            mustAliases.Add(alias);
            alias.mustAliases.Add(this);
        }

        private void UnsetVariable()
        {
            if (mustRepresentant == this)
            {
                Variable mustGroupRepresentant = mustAliases.GetEnumerator().Current;
                
                foreach (Variable mayAlias in mayAliases)
                {
                    if (mayAlias.mayAliases.Contains(this))
                    {
                        mayAlias.mayAliases.Remove(this);
                        mayAlias.mayAliases.Add(mustGroupRepresentant);
                    }
                }

                foreach (Variable mustAlias in mustAliases)
                {
                    mustAlias.mustAliases.Remove(this);
                    mustAlias.mustRepresentant = mustGroupRepresentant;
                }
            }
            else
            {
                foreach (Variable mayAlias in mayAliases)
                {
                    if (mayAlias.mayAliases.Contains(this))
                    {
                        mayAlias.mayAliases.Remove(this);
                    }
                }

                foreach (Variable mustAlias in mustAliases)
                {
                    mustAlias.mustAliases.Remove(this);
                }
            }

            reset();
        }

        public Variable CopyStructure(CopyResolver resolver)
        {
            Variable newVar = new Variable();

            if (IsRepresentant)
            {
                foreach (Value value in values)
                {
                    newVar.values.Add(value.CopyStructure(resolver));
                }

                newVar.nextLevel.CopyStructureFrom(nextLevel, resolver);

                if (HasMayAliases || HasMustAliases)
                {
                    resolver.Add(newVar, this);
                }
            }

            return newVar;
        }

        public void MergeCopy(Variable source, CopyResolver resolver)
        {
            throw new NotImplementedException("Dodelat merge pro aliasy - must + merge u instanci trid");

        }
        
        
        
        
        
        
        
        
        
        
        
        
        
        
        
        
        
        
        
        
        
        
        
        
        
        
        
        
        
        
        
        
        
        
        
        
        public void SetValue(Value value)
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

}
