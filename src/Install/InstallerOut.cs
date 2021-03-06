﻿#region Usings

using System;
using System.Text;

#endregion

namespace IHI.Server.Install
{
    internal class InstallerOut
    {
        internal string Title
        {
            set
            {
                int requiredPadding = Console.BufferWidth - value.Length;

                if ((requiredPadding & 1) == 1) // Is RequiredPadding odd?
                {
                    value += " "; // Yes, make it even.
                    requiredPadding--;
                }

                value =
                    value.PadLeft(value.Length + requiredPadding/2)
                        .PadRight(Console.BufferWidth)
                        .PadRight(Console.BufferWidth*2, '=');

                Console.SetCursorPosition(0, 0);
                Console.Write(value);
            }
        }

        internal InstallerOut SetStep(byte current, byte total)
        {
            string text = current + "/" + total;

            Console.SetCursorPosition(0, 2);
            Console.Write(text.PadLeft(Console.BufferWidth));
            return this;
        }

        internal InstallerOut SetStatus(string text, ConsoleColor foreground = ConsoleColor.Gray,
                                      ConsoleColor background = ConsoleColor.Black)
        {
            Console.ForegroundColor = foreground;
            Console.BackgroundColor = background;

            text = text.Length > Console.BufferWidth - 1
                       ? text.Substring(0, Console.BufferWidth - 1)
                       : text.PadRight(Console.BufferWidth - 1);

            Console.SetCursorPosition(0, Console.BufferHeight - 1);
            Console.Write(text);

            Console.ForegroundColor = ConsoleColor.Gray;
            Console.BackgroundColor = ConsoleColor.Black;
            return this;
        }

        internal InstallerOut ClearPage()
        {
            Console.SetCursorPosition(0, 6);

            StringBuilder blankness = new StringBuilder
                                          {
                                              Length = Console.BufferWidth*(Console.BufferHeight - 7)
                                          };

            Console.Write(blankness.ToString());
            Console.SetCursorPosition(0, 6);
            return this;
        }

        internal InstallerOut OverwritePageContents(string contents)
        {
            ClearPage();
            Console.SetCursorPosition(0, 6);
            Console.Write(contents);
            return this;
        }
    }
}