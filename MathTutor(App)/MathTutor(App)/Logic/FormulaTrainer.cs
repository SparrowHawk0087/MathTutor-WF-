using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using System.Text;
using System.Threading.Tasks;

namespace MathTutor_App_
{
    // Класс тренажёра для заучивания формул
    public class FormulaTrainer
    {
        public Dictionary<string, List<Formula>> FormulaBank { get; set; } = new Dictionary<string, List<Formula>> { };
        public Dictionary<Formula, int> CorrectAnswers { get; } = new Dictionary<Formula, int> { };
        public Dictionary<Formula, int> IncorrectAnswers { get; } = new Dictionary<Formula, int> { };

        private Dictionary<Formula, Queue<bool>> _answerHistory = new();
        private const int MaxHistoryStorage = 1000;

        // конструктор класса
        public FormulaTrainer() { }


        // текстовые сообщения
        private const string greeting = "\n### Тренировка формул ###\nТренировка началась! Желаю быстрого изучения формул)))";
        private const string decsription = "\nПосле показа правильного варианта на экране введите '+',\nесли вспомнили формулу, '-', если ошиблись,\nили 'q', если хотите выйти.";
        private const string warning = "\nК сожалению вы указываете тему, которую изначально не выбрали для тренировки или её не существует в данном тренажере.\nПопробуйте снова:";
        private const string commandMessage = "\nКоманды: 'q' - выход, 't' - выбор темы";
        private const string topicHint = "\nДоступные темы:";
        private const string selectTopicMessage = "Введите тему для отработки (или 'q' для выхода):";
        private const string hint = "\nФормула: {0}\nНажмите Enter, чтобы увидеть ответ";
        private const string answer = "\nОтвет: {0}";
        private const string feedback = "\nВы ответили правильно? (+/-/q): ";
        private const string continueDrillMessage = "\nХотите продолжить тренировку? (y/n):";
        private const string invalidInput = "Некорректный ввод. Пожалуйста, попробуйте снова.";
        private const string switchTopicMessage = "\nВопросы по данной теме закончились. Хотите сменить тему? (y/n):";

        /// <summary>
        /// Метод AddFormula(string topic, Formula formula) добавляет в выбранную тему новую формулу.
        /// </summary>
        /// <param name="topic"></param>
        /// <param name="formula"></param>
        /// <exception cref="ArgumentNullException"></exception>
        public void AddFormula(string topic, Formula formula)
        {

            //  проверка, что переданная формула не null
            if (formula == null)
                throw new ArgumentNullException(nameof(formula));

            // Если темы нет - создаём список формул для неё
            if (!FormulaBank.ContainsKey(topic))
                FormulaBank.Add(topic, new List<Formula>());

            // Если формулы нет в теме - добавляем в список формулу
            if (!FormulaBank[topic].Contains(formula))
            {
                FormulaBank[topic].Add(formula);
                CorrectAnswers[formula] = 0;
                IncorrectAnswers[formula] = 0;
                return;
            }

            Console.WriteLine($"Формула '{formula.Name}' уже существует в теме '{topic}'. Пропускаем добавление.");
        }

        /// <summary>
        /// Метод LoadFormulaFromFile(string filepath) выполняет загрузку формул из файла.
        /// </summary>
        /// <param name="filepath"></param>
        public void LoadFormulaFromFile(string filepath)
        {
            try
            {
                string[] info = File.ReadAllLines(filepath, Encoding.UTF8);

                foreach (string line in info)
                {
                    var keys = line.Split('|');

                    // проверка на соответсвие строки по количеству meta-data
                    if (keys.Length != 3) continue;

                    AddFormula(keys[0], new Formula(keys[1], keys[2]));

                }
            }

            catch (FileNotFoundException e)
            {
                Console.WriteLine($"Ошибка открытия файла: {e.Message}");
            }
        }

        /// <summary>
        /// Метод GetListOfTopics() возвращает список тем из банка формул.
        /// </summary>
        /// <returns></returns>
        public List<string> GetListOfTopics()
        {
            return FormulaBank.Keys.ToList();
        }


        /// <summary>
        /// Метод StartDrill(List<string> Mytopics) организовывает всю работу тренажера для заучивания.
        /// </summary>
        /// <param name="Mytopics"></param>
        public void StartDrill(List<string> availableTopics)
        {

            if (availableTopics.Count == 0 || availableTopics == null)
            {
                Console.WriteLine("Нет формул для выбранных тем.");
                return;
            }


            // старт тренировки
            Console.WriteLine(greeting);
            Console.WriteLine(decsription);
            Console.WriteLine(commandMessage);

            bool continueDrill = true;
            string? currentTopic = null;

            // выбор темы из доступных для отработки
            while (continueDrill)
            {
                currentTopic = SelectTopic(availableTopics);

                if (currentTopic == null) break;

                // получение формул выбранной темы + сортировка по частоте ошибок 
                var trainingQueque = FormulaBank[currentTopic].OrderByDescending(f => GetErrorRate(f)).ToList();

                // тренировка по выбранной теме
                foreach (var formula in trainingQueque)
                {
                    // проверка ввода пользователя после вывода формулы.
                    if (!CheckFormula(formula, ref continueDrill))
                        break;
                }

                // прошли все вопросы по теме
                if (continueDrill)
                {
                    continueDrill = AskToContinueDrill();

                    if (continueDrill)
                    {
                        // смена темы после завершения тренировки по текущей теме или продолжение в текущей теме
                        if (AskToChangeTopic())
                            currentTopic = null;
                    }
                }

            }
            Console.WriteLine("\nТренировка завершена. Продолжайте в том же духе!");

        }

        /// <summary>
        /// Метод SelectTopic(List<string> availableTopics) реализует выбор текущей темы из доступных на отработку.
        /// </summary>
        /// <param name="availableTopics"></param>
        /// <returns></returns>
        private string SelectTopic(List<string> availableTopics)
        {
            while (true)
            {
                Console.WriteLine(topicHint);
                Console.WriteLine(string.Join(",", availableTopics));
                Console.WriteLine(selectTopicMessage);

                // считываем ввод темы от пользователя или команду 'q'
                string currentTopic = Console.ReadLine()?.Trim();

                if (string.IsNullOrEmpty(currentTopic))
                {
                    Console.WriteLine(invalidInput);
                    continue;
                }

                if (currentTopic.ToLower() == "q")
                    return null;

                if (availableTopics.Contains(currentTopic))
                    return currentTopic;

                Console.WriteLine(warning);
            }
        }

        /// <summary>
        /// Метод CheckFormula(Formula formula, ref bool continueDrill) проверяет ответ пользователя при проверки формулы в тренажере.
        /// </summary>
        /// <param name="formula"></param>
        /// <param name="continueDrill"></param>
        /// <returns></returns>
        public bool CheckFormula(Formula formula, ref bool continueDrill)
        {
            Console.WriteLine(hint, formula.Name);

            while (true)
            {
                var msg = Console.ReadLine()?.Trim().ToLower();
                if (msg == "q")
                {
                    continueDrill = false;
                    return false;
                }

                if (msg == "t") return false;

                if (string.IsNullOrEmpty(msg)) break; // Пропускаем пустой ввод


            }

            // вывод правильного ответа
            Console.WriteLine(answer, formula.Answer);

            while (true)
            {
                // учет ответа пользователя
                Console.WriteLine(feedback);
                var selfCheck = Console.ReadLine()?.ToLower();

                switch (selfCheck)
                {
                    case "+":
                        RecordAnswerIntoHistory(formula, true);
                        return true;

                    case "-":
                        RecordAnswerIntoHistory(formula, false);
                        return true;

                    case "t":
                        return false;

                    case "q":
                        continueDrill = false;
                        return false;

                    default:
                        Console.WriteLine(invalidInput);
                        break;
                }
            }
        }

        /// <summary>
        /// Метод GetUserInput(List<string> availableOptions) получает ответ пользователя при проверке формул на запоминание. 
        /// </summary>
        /// <param name="availableOptions"></param>
        /// <returns></returns>
        private string GetUserInput(List<string> availableOptions)
        {
            while (true)
            {
                string? msg = Console.ReadLine()?.Trim().ToLower();

                if (!string.IsNullOrEmpty(msg) && availableOptions.Contains(msg)) return msg;
                Console.WriteLine(invalidInput);
            }
        }

        /// <summary>
        /// Метод AskToContinueDrill() учитывает желание пользователя продолжить тренировку или её завершить.
        /// </summary>
        /// <returns></returns>
        private bool AskToContinueDrill()
        {
            Console.WriteLine(continueDrillMessage);
            return GetYesOrNoInput();
        }

        /// <summary>
        /// Метод AskToChangeTopic() учитывает желание пользователя продолжить тренировку по выбранной теме или сменить её.
        /// </summary>
        /// <returns></returns>
        private bool AskToChangeTopic()
        {
            Console.WriteLine(switchTopicMessage);
            return GetYesOrNoInput();
        }

        /// <summary>
        /// Метод GetYesOrNoInput() считывает ответ пользователя 'y' или 'n'.
        /// </summary>
        /// <returns></returns>
        private bool GetYesOrNoInput()
        {
            while (true)
            {
                string? msg = Console.ReadLine()?.Trim().ToLower();
                if (msg == "y") return true;
                if (msg == "n") return false;
                Console.WriteLine(invalidInput);
            }
        }

        /// <summary>
        /// Метод RecordAnswerIntoHistory(Formula formula, bool isCorrect) записи ответов пользователя
        /// </summary>
        /// <param name="formula"></param>
        /// <param name="isCorrect"></param>
        public void RecordAnswerIntoHistory(Formula formula, bool isCorrect)
        {
            if (!_answerHistory.ContainsKey(formula))
                _answerHistory.Add(formula, new Queue<bool>());


            if (_answerHistory[formula].Count >= MaxHistoryStorage)
                _answerHistory[formula].Enqueue(isCorrect);

            _answerHistory[formula].Enqueue(isCorrect);
        }

        /// <summary>
        /// Метод GetErrorRate(Formula formula) выдает процент неправильных ответов по формуле.
        /// </summary>
        /// <param name="formula"></param>
        /// <returns></returns>
        private double GetErrorRate(Formula formula)
        {
            // очередь ответов по данной формуле
            if (!_answerHistory.TryGetValue(formula, out var history) || !history.Any())
                return 0.0; //истории нет или она пуста

            // подсчёт кол-ва неправильных ответов
            int errorAnswers = history.Count(answer => !answer);

            // выдача процента неправильных ответов
            return (double)errorAnswers / history.Count;
        }

        /// <summary>
        /// Метод GetStatics() отвечает за выдачу статистики пользователя по прохождении тренажера.
        /// </summary>
        public void GetStatics(int? lastAttempts = null)
        {
            Console.WriteLine("\n|~~~ Статистика ~~~|");

            Console.WriteLine(lastAttempts.HasValue
               ? $"Учитываются последние {lastAttempts} ответов"
               : "Учитываются все ответы");

            foreach (var topic in FormulaBank.Keys)
            {
                Console.WriteLine($"\nТема: {topic}");
                // получение статистики по теме
                var statics = CalculateTopicStats(topic, lastAttempts);

                // вывод общей информации о теме(кол-во ответов, кол-во правильных-неправильных ответов и их процентное соотношение)
                Console.WriteLine($"Всего ответов: {statics.totalAttempts}");
                Console.WriteLine($"\nКоличество правильных ответов: {statics.correct} ({statics.correctProportion}%)");
                Console.WriteLine($"\nКоличество неправильных ответов: {statics.incorrect} ({statics.incorrectProportion}%)");

                // вывод информации о наисложнейших вопросах в теме
                PrintHardFormulas(topic, lastAttempts);
            }
        }
    
        /// <summary>
        /// Метод CalculateTopicStats() (string topic, int? lastAttempts) для темы подсчитывает информацию о правильных-неправильных
        /// ответах по каждой формуле, а также считает процент успешных-провальных ответов и общее кол-во попыток.
        /// </summary>
        /// <param name="topic"></param>
        /// <param name="lastAttempts"></param>
        /// <returns></returns>
        private (int correct, int incorrect, int totalAttempts, double correctProportion, double incorrectProportion) CalculateTopicStats(string topic, int? lastAttempts)
        {
            // список формул по теме из хранилища
            var formulas = FormulaBank[topic];
            var (c, inc) = (0, 0);

            foreach (var formula in formulas)
            {
                // получаем историю ответов по формуле
                var history = GetAnswerHistory(formula, lastAttempts);
                //подсчёт правильных и неправильных ответов по формуле за все попытки
                c += history.Count(answer => answer);
                inc += history.Count(answer => !answer);
            }

            // фиксирование результатов по теме
            int res = c + inc;
            double cProportion = res > 0 ? (double)c / res * 100 : 0;
            double incProportion = res > 0 ? (double)inc / res * 100 : 0;

            return (c, inc, res, cProportion, incProportion);

        }

        /// <summary>
        /// Метод GetAnswerHistory(Formula formula, int? lastAttemps) выполняет извлечение истории ответов для конкретной формулы с возможностью ограничения по количеству последних ответов.
        /// </summary>
        /// <param name="formula"></param>
        /// <param name="lastAttemps"></param>
        /// <returns></returns>
        public IEnumerable<bool> GetAnswerHistory(Formula formula, int? lastAttempts)
        {
            if (!_answerHistory.TryGetValue(formula, out var history))
                return Enumerable.Empty<bool>();

            return lastAttempts.HasValue
                ? history.TakeLast(Math.Min(lastAttempts.Value, history.Count))
                : history;
        }

        /// <summary>
        /// Метод PrintHardFormulas(string topic, int? lastAttempts) 
        /// выводит информацию о самых трудных формулах, с которыми
        /// столкнулся пользователь.
        /// </summary>
        /// <param name="topic"></param>
        /// <param name="lastAttempts"></param>
        public void PrintHardFormulas(string topic, int? lastAttempts)
        {
            // выбор формул из темы, преобразование в List, фильтрация формул с ошибками, а затем сортировка по количеству ошибок
            var problemFormulas = FormulaBank[topic]
                .Select(f => new { Formula = f, History = GetAnswerHistory(f, lastAttempts).ToList() })
                .Where(x => x.History.Any(answer => !answer))
                .OrderByDescending(elem => elem.History.Count(answer => !answer))
                .Take(3);

            // проверка на наличие результатов
            if (!problemFormulas.Any()) return;

            Console.WriteLine("\nСамые сложные формулы:");
            foreach (var hardFormula in problemFormulas)
            {
                int inc = hardFormula.History.Count(answer => !answer);
                int res = hardFormula.History.Count();
                double proportion = (double)inc / res * 100;
                Console.WriteLine($"{hardFormula.Formula.Name}: {inc} не верных ответов из {res} ({proportion:f2}%)");
            }
        }
    }
}