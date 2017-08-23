using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

/*
0   ────╮                     ╔            ╗
1   Turn: xx│  Deck: xx  Hand: xx ║ PlayerName ║     xx/xx ●●●●●◎◎◎○○
2   ────╯        Cemetery: xx ║   HP: xx   ║
3                                  ╚            ╝
4      ╔            ╦            ╔ 9┌───╮╗            ╦            ╗   
5      ║            ║            ║┌╯      │║            ║            ║   
6                                    │CardName│                                 
7                                    │CardName│                                 
8                                    ├─╮╭─┤                                 
9                                    │xx├┤xx│
10     ║            ║            ║╰─╯╰─╯║            ║            ║   
11     ╚            ╩            ╚ ﹌﹌﹌﹌﹌ ╝            ╩            ╝   
12
13     ╔            ╦            ╦            ╦            ╦            ╗   
14     ║            ║            ║ 9┌───╮║            ║            ║
15                                   ┌╯      │
16                                   │CardName│                                 
17                                   │CardName│                                 
18                                   ├─╮╭─┤                                 
19     ║            ║            ║│xx├┤xx│║            ║            ║   
20     ╚            ╩            ╩╰─╯╰─╯╩            ╩            ╝   
21                                 ╔            ╗
22                    Cemetery: xx ║   HP: xx   ║
23              Deck: xx  Hand: xx ║ PlayerName ║     xx/xx ●●●●●◎◎◎○○
24                                 ╚            ╝
25                 ┌xx xx/xx┐                                                   
26           ┌─┐│CardName│┌─┐┌─┐┌─┐┌─┐┌─┐┌─┐┌─┐         
27           │  ││CardName││  ││  ││  ││  ││  ││  ││  │       
28  ───────────────────────────────────────
29  > 
*/

namespace HS_sim {
    class Screen {
        private static string[] focusFrame = new string[] { "╔", "║", "╚", "╗", "║", "╝", "  ", "  ", "  " };
        private string[] content = new string[29];
        private string[] popup = null;
        private bool dirtyBit = true;

        public Screen() {
            content[28] = new string('─', 39);
            Console.SetWindowSize(80, 32);
            Console.SetBufferSize(80, 32);
        }

        public void Repaint() {
            if (dirtyBit) {
                Console.Clear();
                if (popup == null) {
                    for (int i = 0; i < 29; ++i) {
                        Console.WriteLine(content[i]);
                    }
                } else {
                    int marginTop = (29 - popup.Length - 2) / 2;
                    int i = 0;
                    for (; i < marginTop; ++i) {
                        Console.WriteLine(content[i]);
                    }
                    for (int j = 0; j < popup.Length; ++j) {
                        Console.WriteLine(popup[j]);
                        ++i;
                    }
                    for (; i < 29; ++i) {
                        Console.WriteLine(content[i]);
                    }
                }
                
            }
            dirtyBit = false;
        }

        public void ShowPrompt() {
            ShowPrompt("");
        }

        public void ShowPrompt(string prompt) {
            Console.SetCursorPosition(0, 29);
            Console.Write("{0}> ", prompt);
        }

        public void HidePrompt() {
            Console.SetCursorPosition(0, 29);
            Console.Write(new string(' ', Console.WindowWidth));
            Console.SetCursorPosition(0, 29);
        }

        public void ShowPopup(string[] msg) {
            dirtyBit = true;

            if (msg == null) {
                popup = new string[] {
                    "      ┌───────────────────────────────┐      ",
                    "      └───────────────────────────────┘      "
                };
            } else {
                popup = new string[msg.Length + 4];
                popup[0] = "      ┌───────────────────────────────┐      ";
                popup[1] = "";
                for (int i = 0; i < msg.Length; ++i) {
                    popup[2 + i] = (string)msg[i].Clone();
                    centerAlign(ref popup[2 + i], Encoding.Default.GetByteCount(popup[2 + i]), 78);
                }
                popup[popup.Length - 2] = "";
                popup[popup.Length - 1] = "      └───────────────────────────────┘      ";
            }
        }

        public void RefreshOpp(int turnCount, Player.PublicInfo oppInfo, bool focusFace) {
            RefreshOpp(turnCount, oppInfo, focusFace, -1);
        }

        public void RefreshOpp(int turnCount, Player.PublicInfo oppInfo, bool focusFace, int focusBoard) {
            dirtyBit = true;

            string name = (string)oppInfo.face.name.Clone();
            centerAlign(ref name, 12);
            
            string crystal = new string('●', oppInfo.currentCrystal);
            if (oppInfo.maximumCrystal > oppInfo.currentCrystal) crystal = crystal.PadRight(oppInfo.maximumCrystal, '◎');
            crystal = crystal.PadRight(10, '○');

            StringBuilder[] oppBoardBuilders = new StringBuilder[8];
            for (int i = 0; i < 8; ++i) {
                oppBoardBuilders[i] = new StringBuilder();
            }
            buildBoard(oppInfo, focusBoard, oppBoardBuilders);

            content[0] = String.Format("────╮                     {0}            {1}",
                focusFace ? focusFrame[0] : "  ",
                focusFace ? focusFrame[3] : "  "
            );
            content[1] = String.Format(
                "Turn: {0,2}│  Deck: {1,2}  Hand: {2,2} {3}{4}{5}     {6,2}/{7,2} {8}",
                turnCount, oppInfo.deckNumber, oppInfo.handNumber,
                focusFace ? focusFrame[1] : "  ",
                name,
                focusFace ? focusFrame[1] : "  ",
                oppInfo.currentCrystal, oppInfo.maximumCrystal,
                crystal
            );
            content[2] = String.Format(
                "────╯        Cemetery: {0,2} {1}   HP: {2,2}   {3}",
                oppInfo.cemeteryNumber,
                focusFace ? focusFrame[1] : "  ",
                oppInfo.face.hp,
                focusFace ? focusFrame[1] : "  "
            );
            content[3] = String.Format(
                "                               {0}            {1}",
                focusFace ? focusFrame[2] : "  ",
                focusFace ? focusFrame[5] : "  "
            );
            int width = 2 + 14 * oppInfo.board.Count;
            for (int i = 0; i < 8; ++i) {
                content[4 + i] = oppBoardBuilders[i].ToString();
                centerAlign(ref content[4 + i], width, 78);
            }
        }

        public void RefreshSelf(Player.PublicInfo selfInfo, bool focusFace) {
            RefreshSelf(selfInfo, focusFace, -1);
        }

        public void RefreshSelf(Player.PublicInfo selfInfo, bool focusFace, int focusBoard) {
            dirtyBit = true;

            string name = (string)selfInfo.face.name.Clone();
            centerAlign(ref name, 12);

            string crystal = new string('●', selfInfo.currentCrystal);
            if (selfInfo.maximumCrystal > selfInfo.currentCrystal) crystal = crystal.PadRight(selfInfo.maximumCrystal, '◎');
            crystal = crystal.PadRight(10, '○');

            StringBuilder[] selfBoardBuilders = new StringBuilder[8];
            for (int i = 0; i < 8; ++i) {
                selfBoardBuilders[i] = new StringBuilder();
            }
            buildBoard(selfInfo, focusBoard, selfBoardBuilders);

            int width = 2 + 14 * selfInfo.board.Count;
            for (int i = 0; i < 8; ++i) {
                content[13 + i] = selfBoardBuilders[i].ToString();
                centerAlign(ref content[13 + i], width, 78);
            }
            content[21] = String.Format(
                "                               {0}            {1}",
                focusFace ? focusFrame[0] : "  ",
                focusFace ? focusFrame[3] : "  "
            );
            content[22] = String.Format(
                "                  Cemetery: {0,2} {1}   HP: {2,2}   {3}",
                selfInfo.cemeteryNumber,
                focusFace ? focusFrame[1] : "  ",
                selfInfo.face.hp,
                focusFace ? focusFrame[1] : "  "
            );
            content[23] = String.Format(
                "            Deck: {0,2}  Hand: {1,2} {2}{3}{4}     {5,2}/{6,2} {7}",
                selfInfo.deckNumber, selfInfo.handNumber,
                focusFace ? focusFrame[1] : "  ",
                name,
                focusFace ? focusFrame[1] : "  ",
                selfInfo.currentCrystal, selfInfo.maximumCrystal,
                crystal
            );
            content[24] = String.Format(
                "                               {0}            {1}",
                focusFace ? focusFrame[2] : "  ",
                focusFace ? focusFrame[5] : "  "
            );
        }

        private void centerAlign(ref string str, int length, int capacity) {
            int spaceLen = (capacity - length) / 2;
            str = str.PadLeft(str.Length + spaceLen);
            str = str.PadRight(str.Length + spaceLen);
        }

        private void centerAlign(ref string str, int capacity) {
            str = str.PadLeft((capacity + str.Length) / 2);
            str = str.PadRight(capacity);
        }

        private string[] getNameToksForHand(string str) {
            string[] rtn = new string[] { "", "" };
            str = str.Trim();
            if (str.Length == 0) return rtn;

            int j = 0;
            for (int i = 0; i < str.Length; ++i) {
                if (Char.IsWhiteSpace(str, i)) {
                    if (j == 0) j = i;
                    if (i > str.Length / 2) break;
                }
            }

            if (j == 0) {
                rtn[0] = str;
            } else {
                rtn[0] = str.Substring(0, j).Trim();
                rtn[1] = str.Substring(j + 1).Trim();
            }

            int capacity = Math.Max(8, Math.Max(rtn[0].Length, rtn[1].Length));
            centerAlign(ref rtn[0], capacity);
            centerAlign(ref rtn[1], capacity);

            return rtn;
        }

        private string[] getNameToksForBoard(string str) {
            string[] rtn = new string[] { "", "" };
            str = str.Trim();
            if (str.Length == 0) return rtn;

            int j = 0;
            for (int i = 0; i < str.Length; ++i) {
                if (Char.IsWhiteSpace(str, i)) {
                    if (j == 0) j = i;
                    if (i > 8) break;
                }
            }

            if (j == 0) {
                rtn[0] = str;
            } else {
                rtn[0] = str.Substring(0, j).Trim();
                rtn[1] = str.Substring(j + 1).Trim();
            }

            if (rtn[0].Length > 8) rtn[0] = rtn[0].Substring(0, 7) + "~";
            if (rtn[1].Length > 8) rtn[1] = rtn[1].Substring(0, 7) + "~";

            return rtn;
        }

        private void buildBoard(Player.PublicInfo info, int focus, StringBuilder[] builders) {
            if (info.board.Count == 0) return;
            int firstIdx = (focus == 0) ? 0 : 2;
            builders[0].Append(focusFrame[firstIdx * 3]);
            builders[1].Append(focusFrame[firstIdx * 3 + 1]);
            builders[2].Append("  ");
            builders[3].Append("  ");
            builders[4].Append("  ");
            builders[5].Append("  ");
            builders[6].Append(focusFrame[firstIdx * 3 + 1]);
            builders[7].Append(focusFrame[firstIdx * 3 + 2]);
            for (int i = 0; i < info.board.Count; ++i) {
                BoardObject obj = info.board[i];
                string[] toks = info.board[i].GetDescription().Split(new char[] {','}, 2);
                string[] valueToks = toks[0].Split('/');
                string[] nameToks = getNameToksForBoard(toks[1]);
                int focusIdx = 1 + i - focus;
                if (focusIdx < 0 || focusIdx > 1) focusIdx = 2;
                if (obj is Monster && (obj as Monster).CanAttack()) {
                    builders[0].AppendFormat("{0,2}┌───╮{1}", valueToks[0], focusFrame[focusIdx * 3]);
                    builders[1].AppendFormat("┌╯      │{0}", focusFrame[focusIdx * 3 + 1]);
                    builders[2].AppendFormat("│{0,8}│  ", nameToks[0]);
                    builders[3].AppendFormat("│{0,8}│  ", nameToks.Length > 1 ? nameToks[1] : "");
                    builders[4].Append("├─╮╭─┤  ");
                    builders[5].AppendFormat("│{0,2}├┤{1,2}│  ", valueToks[1], valueToks[2]);
                    builders[6].AppendFormat("╰─╯╰─╯{0}", focusFrame[focusIdx * 3 + 1]);
                    builders[7].AppendFormat(" ︾︾︾︾︾ {0}", focusFrame[focusIdx * 3 + 2]);
                } else {
                    builders[0].AppendFormat("            {0}", focusFrame[focusIdx * 3]);
                    builders[1].AppendFormat("{0,2}┌───╮{1}", valueToks[0], focusFrame[focusIdx * 3 + 1]);
                    builders[2].AppendFormat("┌╯      │  ");
                    builders[3].AppendFormat("│{0,8}│  ", nameToks[0]);
                    builders[4].AppendFormat("│{0,8}│  ", nameToks.Length > 1 ? nameToks[1] : "");
                    if (valueToks.Length > 1) {
                        builders[5].Append("├─╮╭─┤  ");
                        builders[6].AppendFormat("│{0,2}├┤{1,2}│{2}", valueToks[1], valueToks[2], focusFrame[focusIdx * 3 + 1]);
                        builders[7].AppendFormat("╰─╯╰─╯{0}", focusFrame[focusIdx * 3 + 2]);
                    } else {
                        builders[5].Append("│        │  ");
                        builders[6].AppendFormat("└────┘{0}", focusFrame[focusIdx * 3 + 1]);
                        builders[7].AppendFormat("            {0}", focusFrame[focusIdx * 3 + 2]);
                    }
                }
            }
        }

        public void RefreshHand(int number, int idx, string target) {
            dirtyBit = true;

            StringBuilder[] builder = new StringBuilder[3];
            for (int i = 0; i < 3; ++i) {
                builder[i] = new StringBuilder();
            }

            int nameLen = 8;
            for (int i = 0; i < number; ++i) {
                if (idx != i) {
                    builder[0].Append("      ");
                    builder[1].Append("┌─┐");
                    builder[2].Append("│  │");
                } else {
                    string[] toks = target.Split(',');
                    string[] valueToks = toks[0].Split('/');
                    string[] nameToks = getNameToksForHand(toks[1]);
                    nameLen = nameToks[0].Length;
                    if (valueToks.Length > 1) {
                        builder[0].AppendFormat("┌{0,2}{1}{2,2}/{3,2}┐", valueToks[0], new string(' ', nameLen - 7), valueToks[1], valueToks[2]);
                    } else {
                        builder[0].AppendFormat("┌{0,2}{1} ──┐", valueToks[0], new string(' ', nameLen - 7));
                    }
                    builder[1].AppendFormat("│{0}│", nameToks[0]);
                    builder[2].AppendFormat("│{0}│", nameToks[1]);
                }
            }
            for (int i = 0; i < 3; ++i) {
                content[25 + i] = builder[i].ToString();
                centerAlign(ref content[25 + i], 6 * number + (idx == -1 ? 0 : nameLen - 2), 78);
            }
        }
    }
}
