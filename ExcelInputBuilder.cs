using BPO_ex4.StationLogic;
using BPO_ex4.Excel;

namespace BPO_ex4
{
    public static class ExcelInputBuilder
    {
        public static Node[] Build(
    Context ctx,
    string sheet,
    ExcelRow row)
        {
            int n = row.Ist.Length - 1;
            var inputs = new Node[n + 1];

            for (int i = 1; i <= n; i++)
            {
                var id = SourceRules.Resolve(sheet, row.Ist[i], row.Num[i]);
                inputs[i] = ctx.Get(id);
            }

            return inputs;
        }

    }
}
