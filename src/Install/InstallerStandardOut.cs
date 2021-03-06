﻿#region Usings

using System;
using System.Text;

#endregion

namespace IHI.Server.Install
{
    internal class InstallerStandardOut
    {
        internal InstallerStandardOut SetCategoryTitle(string text)
        {
            int requiredPadding = Console.BufferWidth - text.Length;

            if ((requiredPadding & 1) == 1) // Is RequiredPadding odd?
            {
                text += " "; // Yes, make it even.
                requiredPadding--;
            }

            text =
                text.PadLeft(text.Length + requiredPadding/2).PadRight(Console.BufferWidth).PadRight(
                    Console.BufferWidth*2, '=');

            Console.SetCursorPosition(0, 0);
            Console.Write(text);
            return this;
        }

        internal InstallerStandardOut SetStep(byte current, byte total)
        {
            string text = current + "/" + total;

            Console.SetCursorPosition(0, 2);
            Console.Write(text.PadLeft(Console.BufferWidth));
            return this;
        }

        internal InstallerStandardOut SetStatus(string text, ConsoleColor foreground = ConsoleColor.Gray,
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

        internal InstallerStandardOut ClearPage()
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

        internal InstallerStandardOut SetPage(string contents)
        {
            ClearPage();
            Console.SetCursorPosition(0, 6);
            Console.Write(contents);
            return this;
        }
    }
}