/*
CS to GDScript transpiler
by mjt, 2019

*/
using System;
using System.Collections.Generic;

namespace CS2GDScript
{
    class Program
    {
        static void Main(string[] args)
        {
            Console.WriteLine("# CS2GDScript by mjt 2019\n");

            bool writeOrig = true;
            bool firstFunc = false;
            bool comment = false;
            string outstr = "";
            string[] lines;
            List<string> funcs = new List<string>();

            if (args.Length > 0)
            {
                lines = System.IO.File.ReadAllLines(args[0]);
            }
            else
            {
                lines = System.IO.File.ReadAllLines("scripts/Player_kinematic.cs");
            }

            // otetaan funkkarinimet ylös jos sellaisia asetettu (nämä funkkarit muutetaan SomeFunc() -> some_func() )
            foreach (string line in lines)
            {
                if (line.Contains("//@f "))
                    funcs.Add(line.Trim().Split(' ')[1]);
            }

            for (int linenum = 0; linenum < lines.Length; linenum++)
            {
                string line = lines[linenum];
                if (line == "" || line.Contains("using ") || line.Contains("{"))
                    continue;

                if (line.Contains("//@f "))
                    continue;

                outstr += "\n";
                string lvl = GetLevel(line);

                if (line.Contains("}"))
                    continue;

                if (line.Contains("/*"))
                {
                    comment = true;
                    outstr += line.Replace("/*", "\"\"\"");
                    continue;
                }
                if (line.Contains("*/"))
                {
                    comment = false;
                    outstr += line.Replace("*/", "\"\"\"");
                    continue;
                }
                if (comment)
                {
                    outstr += line;
                    continue;
                }

                if (line.Contains("//"))
                    line = line.Replace("//", "#");

                if (line.Contains("class "))
                    line = "extends " + line.Trim().Split(' ')[4];

                if (writeOrig)
                {
                    outstr += "\n" + lvl + "# " + line.Trim() + "\n";
                }

                foreach (string fn in funcs)
                {
                    if (line.Contains(fn))
                    {
                        line = line.Replace(fn, ChangeFuncName(fn));
                    }
                }

                if (line.Contains("new "))
                {
                    line = line.Replace("new ", "");
                }

                if (line.Contains("for(") || line.Contains("for ("))
                {
                    line = "for _x in range(_y):  #TODO fix this";
                }
                else
                if (line.Contains("while(") || line.Contains("while ("))
                {
                    line = line.Replace(")", "):");
                }
                else
                if (line.Contains("if(") || line.Contains("if ("))
                {
                    if (line.Contains(")))"))
                        line = line.Replace(")))", "))):");
                    else
                    if (line.Contains("))"))
                        line = line.Replace("))", ")):");
                    else
                    if (line.Contains(")"))
                        line = line.Replace(")", "):");
                }
                else
                if (line.Contains("else"))
                    line = line.Replace("else", "else:");
                else
                if (line.Contains("(") && line.Contains(")")) // jos funkkari tai ehkä kutsu funkkariin?
                {
                    int start = 0, end = line.IndexOf('(');
                    for (int qq = end; qq > 0; qq--)
                    {
                        // etitään funkkarinimi
                        if (line[qq] == '.' || line[qq] == ' ')
                        {
                            start = qq;
                            break;
                        }
                    }

                    string s = line.Substring(start + 1, end - start);
                    string newword = "";

                    // varattuja sanoja ei muokata     
                    if (s == "Vector2(" || s == "Vector3(" ||
                        s == "Transform2D(" || s == "Transform(")
                    {
                        newword = s;
                    }
                    else
                    if (s.Contains("GetNode"))
                    {
                        newword = s.Replace("GetNode", "get_node");
                        int st = newword.IndexOf("<");
                        if (st > 0)
                        {
                            string neww = newword.Substring(0, st);
                            newword = neww + "(";
                        }
                    }
                    else
                    {
                        newword = ChangeFuncName(s);
                    }

                    line = line.Replace(s, newword);

                    if (lines[linenum + 1].Contains("{"))
                    {
                        line = line.Replace("int ", "");
                        line = line.Replace("float ", "");
                        line = line.Replace("bool ", "");
                        line = line.Replace("string ", "");
                        line = line.Replace("void ", "");

                        line = line.Replace(")", "):");
                        line = "func " + line.Trim();
                        firstFunc = true;
                    }
                }

                line = line.Replace("Math.", "");
                line = line.Replace("public ", "");
                line = line.Replace("private ", "");
                line = line.Replace("override ", "");
                line = line.Replace("int ", "var ");
                line = line.Replace("float ", "var ");
                line = line.Replace("bool ", "var ");
                line = line.Replace("string ", "var ");

                // ennen ekaa metodia, indent=0
                if (firstFunc == false)
                    lvl = "";

                if (line.Contains("const "))
                {
                    line = line.Replace("var ", "").Trim();
                    outstr += lvl + "#TODO put every const to new line\n";
                }

                if (line.Contains("func "))
                    outstr += line.Trim();
                else
                    outstr += lvl + line.Trim();

            }

            Console.WriteLine(outstr);
            //Console.ReadLine();
        }

        static string GetLevel(string line)
        {
            string l = "";
            for (int q = 0; q < line.Length; q++)
                if (line[q] != ' ' && line[q] != '\t')
                    break;
                else l += " ";
            return l;
        }


        static string ChangeFuncName(string s)
        {
            string newword = "";
            for (int i = 0; i < s.Length; i++)
            {
                if (i == 0 && s[i] == '_') continue;

                if (s[i] >= 'A' && s[i] <= 'Z') // iso kirjain
                {
                    // alaviiva tulee ennen isoja kirjaimia kunhan ei ole eka kirjain eikä edellinen kirjain ollut piste
                    if (i > 0 && s[i - 1] != '.')
                        newword += "_";
                }
                newword += s[i];
            }
            newword = newword.ToLower();
            return newword;
        }
    }
}
