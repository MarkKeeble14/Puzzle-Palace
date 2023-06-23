using System.Collections.Generic;

public class MathematicalEquation
{
    private int result;
    private List<int> nums = new List<int>();
    private List<MathematicalOperation> ops = new List<MathematicalOperation>();

    public void Apply(int i)
    {
        nums.Add(i);

        if (nums.Count > 1 && ops.Count > 0)
        {
            Calculate();
        }
    }

    public void Apply(MathematicalOperation op)
    {
        ops.Add(op);

        if (nums.Count > 1 && ops.Count > 0)
        {
            Calculate();
        }
    }

    public void Calculate()
    {
        result = Calculate(nums[0], nums[1], ops[0]);
        ops.RemoveAt(0);
        nums.RemoveAt(0);
        nums.RemoveAt(0);
        List<int> newNums = new List<int>() { result };
        newNums.AddRange(nums);
        nums = newNums;
    }

    private int Calculate(int n1, int n2, MathematicalOperation op)
    {
        switch (op)
        {
            case MathematicalOperation.NONE:
                return 0;
            case MathematicalOperation.ADD:
                return n1 + n2;
            case MathematicalOperation.SUBTRACT:
                return n1 - n2;
            case MathematicalOperation.MULTIPLY:
                return n1 * n2;
            case MathematicalOperation.DIVIDE:
                return n1 / n2;
        }
        return 0;
    }

    public int GetResult()
    {
        return result;
    }
}