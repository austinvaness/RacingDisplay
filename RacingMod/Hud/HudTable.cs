using Sandbox.Game;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using VRage.Utils;
using VRageMath;

namespace avaness.RacingMod.Hud
{
    public class HudTable
    {
        private string id;
        private Vector2D pos, size;
        private Col[] cols;
        private int rows = 0;

        public HudTable(string id, Vector2D pos, Vector2D size, params Col[] cols)
        {
            this.id = id;
            this.pos = pos;
            this.size = size;
            this.cols = cols;
            Create();
        }

        private void Create(long pid = -1)
        {
            MyVisualScriptLogicProvider.CreateBoardScreen(id, (float)pos.X, (float)pos.Y, (float)size.X, (float)size.Y, pid);
            foreach (Col c in cols)
                c.Create(id, pid);
        }

        private void SetData(IEnumerable<Row> rows)
        {
            int i = 0;
            foreach(Row row in rows)
            {
                if (i >= this.rows)
                    AddRow(i);

                SetRow(i, row);
             
                i++;
            }

            for (; i < this.rows; i++)
                DeleteRow(i);
        }

        private void SetRow(int index, Row row)
        {
            string rowId = index.ToString();
            for(int i = 0; i < cols.Length; i++)
            {
                Col col = cols[i];
                string data = "";
                if(i < row.data.Length)
                    data = row.data[i];
                MyVisualScriptLogicProvider.SetCell(id, rowId, col.text, data);
            }
        }

        private void AddRow(int index)
        {
            MyVisualScriptLogicProvider.AddRow(id, index.ToString());
        }

        private void DeleteRow(int index)
        {
            MyVisualScriptLogicProvider.RemoveRow(id, index.ToString());
        }


        public class Row
        {
            public string[] data;

            public Row(params string[] data)
            {
                this.data = data;
            }
        }

        public class Col
        {
            public double size;
            public string text;

            public Col(double size, string text)
            {
                this.size = size;
                this.text = text;
            }

            public void Create(string tableId, long pid)
            {
                MyVisualScriptLogicProvider.AddColumn(tableId, text, (float)size, text, MyGuiDrawAlignEnum.HORISONTAL_LEFT_AND_VERTICAL_CENTER, MyGuiDrawAlignEnum.HORISONTAL_LEFT_AND_VERTICAL_CENTER, pid);
            }
        }
    }
}
