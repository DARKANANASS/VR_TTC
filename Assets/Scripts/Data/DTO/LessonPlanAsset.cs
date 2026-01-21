using System;
using System.Collections.Generic;

[Serializable]
public class LessonPlanAsset
{
    public List<Block> blocks;

    public void AddCondition(string num, ConditionClass condition)
    {
        foreach (Block block in blocks)
        {
            if (block.number == num)
            {
                block.conditions.Add(condition);
                return;
            }
        }
        var b = new Block();
        b.StartBlock(num, condition);
        blocks.Add(b);
    }

    public void Clear()
    {
        blocks = new List<Block>();
    }
}

[Serializable]
public class Block
{
    public string number;
    public List<ConditionClass> conditions = new List<ConditionClass>();

    public void StartBlock(string num, ConditionClass cond)
    {
        number = num;
        conditions.Add(cond);
    }
}