using SetGame.Properties;
using SimpleTCP;
using System;
using System.Drawing;
using System.Windows.Forms;

namespace SetSolver
{
    public partial class Main : Form
    {
        private readonly Label[] cards = new Label[12+4];
        private System.Collections.Generic.Stack<Label> selected = new System.Collections.Generic.Stack<Label>();

        SimpleTcpClient client;
        string name;
        bool activeCheats = false;
        bool showCardID = false;

        bool stop = false;

        private void Main_Load(object sender, EventArgs e)
        {
            client = new SimpleTcpClient();
            client.StringEncoder = System.Text.Encoding.UTF8;
            client.DataReceived += Client_DataReceived;
            connectToolStripMenuItem1.PerformClick();
        }
        private void Client_DataReceived(object sender, SimpleTCP.Message e)
        {
            string msg = e.MessageString;
            Console.WriteLine(msg);
            int msgLen = int.Parse(msg.Substring(0, 4));
            char commandID = msg[4];
            
            msg = msg.Substring(5);
            
            if (commandID == 'B') // state
            {
                Console.WriteLine("Gotten new state");
                
                string[] cds = msg.Split(' ');
                int i = 0;
                for (; i < cds.Length; i++)
                {
                    Label card = cards[i];

                    card.Invoke((MethodInvoker)delegate ()
                    {
                        card.Name = cds[i] + ".png";
                        object O = Resources.ResourceManager.GetObject(cds[i]);
                        card.Image = (Image)O;
                    });
                }

                // rest of the cards, just ignore
                for (; i < cards.Length; i++)
                {
                    cards[i].Image = null;
                    cards[i].BackColor = Color.Transparent;
                    cards[i].Name = "";
                }
                
                Solve_Game();
            }
            else if (commandID == '1') // taken set
            {
                this.stop = true;
                string[] cds = msg.Split(' ');

                int idx1 = int.Parse(cds[0]);
                int idx2 = int.Parse(cds[1]);
                int idx3 = int.Parse(cds[2]);

                cards[idx1].BackColor = Color.HotPink;
                cards[idx2].BackColor = Color.HotPink;
                cards[idx3].BackColor = Color.HotPink;

                System.Threading.Thread.Sleep(2000);
                cards[idx1].BackColor = Color.Transparent;
                cards[idx2].BackColor = Color.Transparent;
                cards[idx3].BackColor = Color.Transparent;

                Solve_Game();
                foreach (var c in selected.ToArray())
                    if (c.Image != null)
                        c.BackColor = Color.Cyan;
                foreach (var card in cards)
                    if (card.Image == null)
                        card.BackColor = Color.Transparent;
                this.stop = false;
            }
            else if(commandID == 'P') // players update
            {
                //string[] players = msg.Split('\n');
                Console.WriteLine("gotten players");
                playersBox.Invoke((MethodInvoker)delegate ()
                {
                    playersBox.Text = msg;
                });
            }
            else if(commandID == 'S')
            {
                Console.WriteLine("gotten new Score");
                scoresBox.Invoke((MethodInvoker)delegate ()
                {
                    scoresBox.Text = msg;
                });
            }
            else if(commandID == 'M')
            {
                Console.WriteLine("gotten new chat message");
                
                chatBox.Invoke((MethodInvoker)delegate ()
                {
                    chatBox.Text += $"{msg}\r\n";
                });
            }
        }

        public Main()
        {
            InitializeComponent();

            cards[0] = result1;
            cards[1] = result2;
            cards[2] = result3;
            cards[3] = result4;
            cards[4] = result5;
            cards[5] = result6;
            cards[6] = result7;
            cards[7] = result8;
            cards[8] = result9;
            cards[9] = result10;
            cards[10] = result11;
            cards[11] = result12;
            cards[12] = result13;
            cards[13] = result14;
            cards[14] = result15;
            cards[15] = result16;

            if(!showCardID)
                foreach (var x in cards)
                    x.Text = "";
            // amountBox.SelectedIndex = 0;
            // shapeBox.SelectedIndex = 0;
            // fillingBox.SelectedIndex = 0;
            // colorBox.SelectedIndex = 0;
        }

        private void label_MouseDown(object sender, MouseEventArgs e)
        {
            Label card = (Label)sender;

            if (card.Image == null || this.stop)
                return;

            if (card.BackColor == Color.Cyan || card.BackColor == Color.LightGreen || card.BackColor == Color.Red)
            {
                card.BackColor = Color.Transparent;
                var pp = selected.ToArray();
                selected = new System.Collections.Generic.Stack<Label>();
                foreach (var z in pp)
                    if (z != card)
                        selected.Push(z);
            }
            else
            {
                if (selected.Count == 3)
                    selected.Pop().BackColor = Color.Transparent;
                selected.Push(card);
            }

            Label[] crds = selected.ToArray();

            if (crds.Length == 3)
            {
                if (IsSet(crds))
                {
                    foreach (var c in crds)
                        c.BackColor = Color.LightGreen;

                    int idx1 = -1;
                    int idx2 = -1;
                    int idx3 = -1;

                    foreach (var c in crds) {
                        for (int i = 0; i < cards.Length; i++)
                        {
                            if(c == cards[i])
                            {
                                if (idx1 == -1)
                                    idx1 = i;
                                else if (idx2 == -1)
                                    idx2 = i;
                                else if (idx3 == -1)
                                    idx3 = i;
                            }
                        }
                    }

                    string msg = "takeset ";
                    msg += $"{idx1} {idx2} {idx3}";
                    msg = getLen(msg) + msg;
                    client.Write(msg);

                    while (selected.Count != 0)
                        selected.Pop();//.BackColor = Color.White;
                }
                else
                    foreach (var c in crds)
                        c.BackColor = Color.Red;
            }
            else
                foreach (var c in crds)
                    c.BackColor = Color.Cyan;
            
            /*
            card.DoDragDrop(card.Image, DragDropEffects.Copy |
                DragDropEffects.Move);

            card.DoDragDrop(card.Name, DragDropEffects.Copy |
                DragDropEffects.Move);*/
        }

        private void ResultImage_DragEnter(object sender, DragEventArgs e)
        {
            if (e.Data.GetDataPresent(DataFormats.Bitmap) || e.Data.GetDataPresent(DataFormats.Text))
                e.Effect = DragDropEffects.Copy;
            else
                e.Effect = DragDropEffects.None;
        }

        private void ResultImage_DragDrop(object sender, DragEventArgs e)
        {
            Label card = (Label)sender;
            if (e.Data.GetDataPresent(DataFormats.Bitmap))
                card.Image = (Image)e.Data.GetData(DataFormats.Bitmap);
            else if (e.Data.GetDataPresent(DataFormats.Text))
            {
                card.Name = (string)e.Data.GetData(DataFormats.Text);
                Solve_Game();
            }
        }

        private void Solve_Game()
        {
            if (!activeCheats)
                return;

            for (int i = 0; i < cards.Length; i++)
                if (cards[i] != null)
                    cards[i].Invoke((MethodInvoker)delegate ()
                    {
                        cards[i].Text = "";
                    });

            var combs = GetKCombs(cards);
            
            int setID = 0;
            setsBox.Invoke((MethodInvoker)delegate ()
            {
                setsBox.Value = 0;
            });

            for (int i = 0; i < combs.Length; i++)
            {
                var tmp = combs[i];
                if (IsSet(tmp))
                {
                    char ID = (char)(65+setID);
                    tmp[0].Invoke((MethodInvoker)delegate ()
                    {
                        tmp[0].Text += ID;
                    });

                    tmp[1].Invoke((MethodInvoker)delegate ()
                    {
                        tmp[1].Text += ID;
                    });

                    tmp[2].Invoke((MethodInvoker)delegate ()
                    {
                        tmp[2].Text += ID;
                    });
                    
                    setsBox.Invoke((MethodInvoker)delegate ()
                    {
                        setsBox.Value++;
                    });
                    
                    setID++;
                }
            }
        }

        private static bool IsSet(Label[] cards)
        {
            //return false;
            Label card1 = cards[0];
            Label card2 = cards[1];
            Label card3 = cards[2];
            if (card1 == null || card2 == null || card3 == null)
            {
                return false;
            }

            string n1 = card1.Name;
            string n2 = card2.Name;
            string n3 = card3.Name;
            if (!n1.Contains("png") || !n2.Contains("png") || !n3.Contains("png"))
            {
                return false;
            }

            n1 = n1.Substring(0, n1.Length-4);
            n2 = n2.Substring(0, n2.Length-4);
            n3 = n3.Substring(0, n3.Length-4);


            string[] properties1 = n1.Split('_');
            string[] properties2 = n2.Split('_');
            string[] properties3 = n3.Split('_');

            // Curve_1_Empty_Green
            //  [0] [1] [2]   [3]

            string shape1 = properties1[0];
            string shape2 = properties2[0];
            string shape3 = properties3[0];

            int amount1 = int.Parse(properties1[1]);
            int amount2 = int.Parse(properties2[1]);
            int amount3 = int.Parse(properties3[1]);

            string filling1 = properties1[2];
            string filling2 = properties2[2];
            string filling3 = properties3[2];

            string color1 = properties1[3];
            string color2 = properties2[3];
            string color3 = properties3[3];
            if(shape1 == shape2 && shape2 == shape3)
            {
                if (amount1 == amount2 && amount2 == amount3)
                {
                    if (filling1 == filling2 && filling2 == filling3) {
                        if (color1 != color2 && color2 != color3 && color1 != color3) // same everything, color diff
                            return true;
                        if (color1 == color2 && color2 == color3)
                            return true;
                    }
                    else if(filling1 != filling2 && filling2 != filling3 && filling1 != filling3)
                    {
                        if (color1 != color2 && color2 != color3 && color1 != color3)
                            return true;
                        if (color1 == color2 && color2 == color3)
                            return true;
                    }
                }
                else if (amount1 != amount2 && amount2 != amount3 && amount1 != amount3)
                {
                    if (filling1 == filling2 && filling2 == filling3)
                    {
                        if (color1 != color2 && color2 != color3 && color1 != color3)
                            return true;
                        if (color1 == color2 && color2 == color3)
                            return true;
                    }
                    else if (filling1 != filling2 && filling2 != filling3 && filling1 != filling3)
                    {
                        if (color1 != color2 && color2 != color3 && color1 != color3)
                            return true;
                        if (color1 == color2 && color2 == color3 && color1 == color3)
                            return true;
                    }
                }
            }
            else if (shape1 != shape2 && shape2 != shape3 && shape1 != shape3)
            {
                if (amount1 == amount2 && amount2 == amount3)
                {
                    if (filling1 == filling2 && filling2 == filling3)
                    {
                        if (color1 != color2 && color2 != color3 && color1 != color3)
                            return true;
                        if (color1 == color2 && color2 == color3 && color1 == color3)
                            return true;
                    }
                    else if (filling1 != filling2 && filling2 != filling3 && filling1 != filling3)
                    {
                        if (color1 != color2 && color2 != color3 && color1 != color3)
                            return true;
                        if (color1 == color2 && color2 == color3 && color1 == color3)
                            return true;
                    }
                }
                else if (amount1 != amount2 && amount2 != amount3 && amount1 != amount3)
                {
                    if (filling1 == filling2 && filling2 == filling3)
                    {
                        if (color1 != color2 && color2 != color3 && color1 != color3)
                            return true;
                        if (color1 == color2 && color2 == color3 && color1 == color3)
                            return true;
                    }
                    else if (filling1 != filling2 && filling2 != filling3 && filling1 != filling3)
                    {
                        if (color1 != color2 && color2 != color3 && color1 != color3)
                            return true;
                        if (color1 == color2 && color2 == color3 && color1 == color3)
                            return true;
                    }
                }
            }
            return false;
        }

        private static Label[][] GetKCombs(Label[] arr)
        {
            Label[][] combs = new Label[560][];
            
            int combsIdx = 0;
            for(int i = 0; i < arr.Length; i++)
            {
                for(int j = i+1; j < arr.Length; j++)
                {
                    for(int k = j+1; k < arr.Length; k++)
                    {
                        Label[] tmp = new Label[3];
                        tmp[0] = arr[i];
                        tmp[1] = arr[j];
                        tmp[2] = arr[k];
                        combs[combsIdx] = tmp;
                        combsIdx++;
                    }
                }
            }

            return combs;
        }

        private void connectToolStripMenuItem_Click(object sender, EventArgs e)
        {
            client = new SimpleTcpClient();

            client.StringEncoder = System.Text.Encoding.UTF8;
            client.DataReceived += Client_DataReceived;
            string promptValue = ConnectForm.ShowDialog();
            client.Connect(promptValue.Split(':')[0], int.Parse(promptValue.Split(':')[1]));

            string msg;
            
            msg = "state";
            msg = "0005" + msg;
            client.Write(msg);

            name = promptValue.Split(':')[2];
            msg = "changename " + name;
            client.Write(getLen(msg) + msg);
        }

        private string getLen(string msg)
        {
            string len = "";
            if (msg.Length > 999);
            else if (msg.Length > 99)
                len += "0";
            else if (msg.Length > 9)
                len += "00";
            else
                len += "000";
            return len + msg.Length;
        }

        private void sendBtn_Click(object sender, EventArgs e)
        {
            messageBox.Text = messageBox.Text.Trim();
            if (messageBox.Text.Length < 1)
                return;
            string data = "chatmsg " + messageBox.Text;
            client.Write(getLen(data) + data);
            messageBox.Text = "";
        }

        private void messageBox_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.KeyCode == Keys.Enter)
            {
                sendBtn.PerformClick();
                e.SuppressKeyPress = true;
                e.Handled = true;
            }
        }

        private void chatBox_TextChanged(object sender, EventArgs e)
        {
            chatBox.SelectionStart = chatBox.Text.Length;
            chatBox.ScrollToCaret();
        }

        private void closeChatBtn_Click(object sender, EventArgs e)
        {
            
            if (closeChatBtn.Text == "X")
            {
                chatBox.Visible = false;
                messageBox.Visible = false;
                sendBtn.Visible = false;

                closeChatBtn.Location = new Point(12, 840);
                closeChatBtn.Text = "O";
            }
            else 
            {
                chatBox.Visible = true;
                messageBox.Visible = true;
                sendBtn.Visible = true;

                closeChatBtn.Location = new Point(12, chatBox.Location.Y-15); 
                closeChatBtn.Text = "X";
            }
        }

    }
}

public static class ConnectForm
{
    public static string ShowDialog()
    {
        Form prompt = new Form()
        {
            Width = 300,
            Height = 260,
            FormBorderStyle = FormBorderStyle.FixedDialog,
            Text = "Connect To Server",
            StartPosition = FormStartPosition.CenterScreen
        };
        Label ipLabel = new Label() { Left = 50, Top = 20, Text = "Ip:"};
        TextBox ipBox = new TextBox() { Left = 50, Top = 40, Width = 200, Text= "0.0.0.0", Enabled=true};
        
        Label portLabel = new Label() { Left = 50, Top = 70, Text = "Port:" };
        TextBox portBox = new TextBox() { Left = 50, Top = 90, Width = 200 , Text="1337", Enabled=true};

        Label nameLabel = new Label() { Left = 50, Top = 120, Text = "Name:" };
        TextBox nameBox = new TextBox() { Left = 50, Top = 145, Width = 200, MaxLength=13};

        Button confirmation = new Button() { Text = "Join", Left = 150, Width = 100, Top = 190, DialogResult = DialogResult.OK};
        confirmation.Click += (sender, e) => { prompt.Close(); };
        prompt.Controls.Add(nameBox);
        prompt.Controls.Add(confirmation);
        prompt.Controls.Add(ipBox);
        prompt.Controls.Add(portBox);
        prompt.Controls.Add(ipLabel);
        prompt.Controls.Add(portLabel);
        prompt.Controls.Add(nameLabel);
        
        prompt.AcceptButton = confirmation;

        return prompt.ShowDialog() == DialogResult.OK ? ipBox.Text+":"+portBox.Text+":"+nameBox.Text : "";
    }
}
