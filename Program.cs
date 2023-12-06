using System.ComponentModel;
using System.Diagnostics.Metrics;

namespace Brackets
{


    public enum Brackets
    {
        open_round = '(',
        close_round = ')',
        open_sq = '[',
        close_sq = ']',
        open_fig = '{',
        close_fig = '}' ,
        nothing = ' '



    }

    class Bracket_broker
    {
        readonly public Brackets bracket;
        public int Counter = 0;

        public Bracket_broker(Brackets _bracket)
        {

            bracket = _bracket;
            Counter = 1;    
        }

        //конструктор скобок
        public Bracket_broker(char c)
        {

         
            bracket = (c == (char)Brackets.open_round) ? Brackets.open_round :
                (c == (char)Brackets.open_sq) ? Brackets.open_sq :
                (c == (char)Brackets.open_fig) ? Brackets.open_fig :

                (c == (char)Brackets.close_fig) ? Brackets.close_fig :
                (c == (char)Brackets.close_round) ? Brackets.close_round :
                (c == (char)Brackets.close_sq) ? Brackets.close_sq : Brackets.nothing;

                Counter=0;
                //считаем скобы
                if (bracket != Brackets.nothing)
                    Counter++;
        }


        /// <summary>
        /// Проверка скоба
        /// </summary>
        /// <param name="c"></param>
        /// <returns>Проверка: скобка?</returns>
        public bool is_Bracket (char c)
        {
                return (c == (char)bracket);

        }

        /// <summary>
        /// Посчитать скобку - 
        /// </summary>
        /// <param name="c"></param>
        /// <returns>Посчитано да/нет</returns>
        public bool count_Bracket(char c)
        {
            if (c == (char)bracket)
                    {
                        Counter++;
                        return true;
                    }

            return (!true);
        }

        public bool Equal(char c)
        {
                return ((c == (char)bracket));
        }

        public bool is_nothing()
        {
            return (this.bracket.Equals( Brackets.nothing));
        }

    }

    //Парсер скобок
    public class BracketsParser
    {
        
        
        //Логика на закрытие скобок
        //должны быть открывашки и закрывашик иначе косяк
        private string MSG = "";


        public string DEBUG { get { return MSG;  } }


        private bool is_closed =false;
        
        public bool CLOSED { get { return is_closed; } }

        public  BracketsParser(string line) 
        {
            //Все. Ок.
            is_closed = true;

            //список скобок в строке
            List<Bracket_broker> brackets = new List<Bracket_broker>();

            //всего элементов
            int all_elements = 0;
            
            foreach (char c in line) 
            {

                Bracket_broker br = new Bracket_broker(c);
                if (br.is_nothing())
                {
                    continue;
                }

                //Проверка на кол-во скобок
                all_elements++;

                //Поиск существующей
                //составялем список использованных скобок
                try
                {
                    Bracket_broker target = brackets.Where(x => x.Equals(br)).First();
                    if (target != null)
                        target.count_Bracket(c);
                    else
                        brackets.Add(br);
                }
                catch (InvalidOperationException ex)
                {
                    brackets.Add(br);

                }
                catch (Exception ex)
                {
                  MSG += ex.ToString()+"\r\n";
                }
                

            } //foreach


            if (all_elements == 0)
            {
                is_closed = false;
                MSG = "Нет скобок в тестовом примере ([{}]).";
                return;
            }

            //открытая скоба
            int opened = 0;
            int closed = 0;

            //Позиция ошибки
            int pos = 0;

            //Контроль для четкой очередности скобок
            List<Bracket_broker> control_brackets = new List<Bracket_broker>();
            
            control_brackets.Add(new Bracket_broker(Brackets.open_sq));
            control_brackets.Add(new Bracket_broker(Brackets.close_sq));

            control_brackets.Add(new Bracket_broker(Brackets.open_fig));
            control_brackets.Add(new Bracket_broker(Brackets.close_fig));

            control_brackets.Add(new Bracket_broker(Brackets.open_round));
            control_brackets.Add(new Bracket_broker(Brackets.close_round));
            

            foreach (Bracket_broker br in brackets) 
            {
                if     (
                           (br.bracket == Brackets.open_sq) 
                        || (br.bracket == Brackets.open_round)  
                        || (br.bracket == Brackets.open_fig)
                        )
                {
                    opened++;
                    //контроль того что открыто
                    control_brackets.Where(x => x.bracket == br.bracket).First().Counter++;

                    continue;

                }
                  
                if (br.bracket == Brackets.close_sq) 
                {
                    closed++;

                    int was_open = control_brackets.Where(x => x.bracket == Brackets.open_sq).First().Counter;
                    int was_close = control_brackets.Where(x => x.bracket == Brackets.close_sq).First().Counter +1 ;

                    control_brackets.Where(x => x.bracket == Brackets.close_sq).First().Counter = was_close;

                    if ((closed > opened) || ( was_open<was_close))
                    {
                        MSG += "\r\nНе закрыты скобки [.  Позиция  " + pos.ToString();
                        is_closed = false;
                        break;
                    }
                }
                else
                if (br.bracket == Brackets.close_round)
                {
                    closed++;


                    int was_open = control_brackets.Where(x => x.bracket == Brackets.open_round).First().Counter;
                    int was_close = control_brackets.Where(x => x.bracket == Brackets.close_round).First().Counter + 1;

                    control_brackets.Where(x => x.bracket == Brackets.close_round).First().Counter = was_close;

                    if ((closed > opened) || (was_open < was_close))
                    { 
                        MSG += "\r\nНе закрыты скобки (.  Позиция  " + pos.ToString();
                        is_closed = false;
                        break;
                    }
                }
                else
                if (br.bracket == Brackets.close_fig)
                {
                    closed++;

                    int was_open = control_brackets.Where(x => x.bracket == Brackets.open_fig).First().Counter;
                    int was_close = control_brackets.Where(x => x.bracket == Brackets.close_fig).First().Counter + 1;

                    control_brackets.Where(x => x.bracket == Brackets.close_fig).First().Counter = was_close;

                    if ((closed > opened) || (was_open < was_close))
                    {
                       MSG += "\r\nНе закрыты скобки {. Позиция  " + pos.ToString();
                        is_closed = false;
                        break;
                     }
                }


                if (opened == closed)
                { 
                 //сброс  {   {  }  }
                 //       +1 +2  -1  
                 opened--;
                 closed--;
                }

                pos++;
            }//foreach

             //последняя проверка
            if (is_closed)
            {
                is_closed = opened == closed; //баланс должен выполняться
            }

            

            if (is_closed) { MSG = "Все в порядке"; }

        }//constructor
    } //class

    internal class Program
    {
        static void Main(string[] args)
        {

            List<string> tests = new List<string>();
            tests.Add("{}");
            tests.Add("[]");
            tests.Add("()(sdksodo())");

            tests.Add("{}}");
            tests.Add("()([])");
            tests.Add("sadoisadois");
            tests.Add("(+}{)");
            tests.Add(")(+}{");
            tests.Add("(()()())");
            tests.Add("(((())");




            foreach (string test in tests) 
            {
                BracketsParser test_check = new BracketsParser(test);
                Console.WriteLine($" {test} -> {test_check.CLOSED} :: {test_check.DEBUG}  ");
                
            }
            

        }
    }
}
