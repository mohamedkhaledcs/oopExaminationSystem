using System;
using System.IO;

namespace ExaminationSystem
{
    // ===================== Student =====================
    class Student
    {
        public string Name { 
            get; set; 
        }

        public int ID { 
            get; set; 
        }


        public Student(string name, int id)
        {
            Name = name;
            ID = id;
        }

    }


    class StudentInfoPrinter
    {
        public static void Print(Student s)
        {
            Console.WriteLine("========================");
            Console.WriteLine("===== Student Info =====");
            Console.WriteLine("========================");
            Console.WriteLine($"Name: {s.Name}");
            Console.WriteLine($"ID: {s.ID}");
            Console.WriteLine("========================\n");

        }

    }


    // ===================== Answer =====================
    class Answer
    {
        public string Text { 
            get; set; 
        }

        public bool IsCorrect { 
            get; set; 
        }


        public Answer(string text, bool isCorrect = false)
        {
            Text = text;
            IsCorrect = isCorrect;

        }

    }


    class AnswerList
    {
        private Answer[] answers;
        private int count;

        public AnswerList(int size = 5)
        {
            answers = new Answer[size];
            count = 0;

        }


        public void Add(Answer a)
        {
            if (count == answers.Length)
            {
                Array.Resize(ref answers, answers.Length * 2);

            }

            answers[count++] = a;

        }


        public Answer[] GetAll() => answers[..count];

    }


    // ===================== Question =====================
    abstract class Question
    {
        public string Header { 
            get; set; 
        }

        public string Body { 
            get; set; 
        }

        public int Marks { 
            get; set; 
        }

        public AnswerList Answers { 
            get; set; 
        }


        protected Question(string header, string body, int marks)
        {
            Header = header;
            Body = body;
            Marks = marks;
            Answers = new AnswerList();

        }


        public abstract void Show();

    }


    class TrueFalseQuestion : Question
    {
        public TrueFalseQuestion(string body, int marks, bool correctAnswer)
            : base("True/False", body, marks)
        {
            Answers.Add(new Answer("True", correctAnswer));
            Answers.Add(new Answer("False", !correctAnswer));

        }

        public override void Show()
        {
            Console.WriteLine("========================");
            Console.WriteLine($"[{Header}] {Body} ({Marks} marks)");
            Console.WriteLine("========================");
            int i = 1;
            foreach (var a in Answers.GetAll())
                Console.WriteLine($"{i++}) {a.Text}");

        }

    }


    class ChooseOneQuestion : Question
    {
        public ChooseOneQuestion(string body, int marks, string[] options, int correctIndex)
            : base("Choose One", body, marks)
        {
            for (int i = 0; i < options.Length; i++)
                Answers.Add(new Answer(options[i], i == correctIndex));

        }

        public override void Show()
        {
            Console.WriteLine("========================");
            Console.WriteLine($"[{Header}] {Body} ({Marks} marks)");
            Console.WriteLine("========================");
            int i = 1;
            foreach (var a in Answers.GetAll())
                Console.WriteLine($"{i++}) {a.Text}");

        }

    }


    class ChooseAllQuestion : Question
    {
        public ChooseAllQuestion(string body, int marks, string[] options, int[] correctIndexes)
            : base("Choose All", body, marks)
        {
            for (int i = 0; i < options.Length; i++)
                Answers.Add(new Answer(options[i], Array.Exists(correctIndexes, idx => idx == i)));

        }

        public override void Show()
        {
            Console.WriteLine("========================");
            Console.WriteLine($"[{Header}] {Body} ({Marks} marks)");
            Console.WriteLine("========================");
            int i = 1;
            foreach (var a in Answers.GetAll())
                Console.WriteLine($"{i++}) {a.Text}");

        }

    }


    // ===================== QuestionList =====================
    class QuestionList
    {
        private Question[] questions;
        private int count;
        private string logFile;

        public QuestionList(string logFile, int size = 10)
        {
            questions = new Question[size];
            count = 0;
            this.logFile = logFile;

        }


        public void Add(Question q)
        {
            if (count == questions.Length)
                Array.Resize(ref questions, questions.Length * 2);

            questions[count++] = q;

            using StreamWriter writer = new StreamWriter(logFile, true);
            writer.WriteLine($"{DateTime.Now}: Added question -> {q.Body}");

        }


        public Question[] GetAll() => questions[..count];

    }


    // ===================== Exam =====================
    abstract class Exam
    {
        public int Time { 
            get; set; 
        }

        public int NumberOfQuestions { 
            get; set; 
        }

        public QuestionList Questions { 
            get; set; 
        }

        protected int Score { 
            get; set; 
        }

        protected int TotalMarks { 
            get; set; 
        }


        protected Exam(int time, QuestionList qList)
        {
            Time = time;
            Questions = qList;
            NumberOfQuestions = qList.GetAll().Length;
            Score = 0;
            TotalMarks = 0;
            foreach (var q in Questions.GetAll())
                TotalMarks += q.Marks;

        }


        public abstract void ConductExam();

        protected bool CheckAnswer(Question q, string input)
        {
            if (string.IsNullOrWhiteSpace(input)) return false;

            var userAns = input.Split(',', StringSplitOptions.RemoveEmptyEntries);
            for (int i = 0; i < userAns.Length; i++)
                userAns[i] = userAns[i].Trim();

            var correctAns = Array.ConvertAll(GetCorrectIndexes(q), x => x.ToString());

            Array.Sort(userAns);
            Array.Sort(correctAns);

            // at return we will compare how stud ans == right ans or not 
            return userAns.Length == correctAns.Length &&
                   string.Join(",", userAns) == string.Join(",", correctAns);
        }

        protected int[] GetCorrectIndexes(Question q)
        {
            var list = q.Answers.GetAll();
            var res = new System.Collections.Generic.List<int>();
            for (int i = 0; i < list.Length; i++)
                if (list[i].IsCorrect)
                    res.Add(i + 1); 

            return res.ToArray();

        }

        protected void PrintResult()
        {
            Console.WriteLine("========================");
            Console.WriteLine("====== Exam Result =====");
            Console.WriteLine("========================");
            Console.WriteLine($"Your mark is: {Score} marks.");

            if (Score == TotalMarks)
            {
                Console.WriteLine("Congratulations! Full Mark!");

            }

        }

    }


    class PracticeExam : Exam
    {
        public PracticeExam(int time, QuestionList qList) : base(time, qList) { }

        public override void ConductExam()
        {
            Console.WriteLine("==== Practice Exam ====");
            foreach (var q in Questions.GetAll())
            {
                q.Show();
                Console.Write("Your Answer (comma for multiple): ");
                string ans = Console.ReadLine();

                if (CheckAnswer(q, ans))
                    Score += q.Marks;

                Console.ForegroundColor = ConsoleColor.Green;
                Console.WriteLine("Correct Answer(s):");
                foreach (var a in q.Answers.GetAll())
                    if (a.IsCorrect)
                        Console.WriteLine($"- {a.Text}");
                Console.ResetColor();
                Console.WriteLine();

            }

            PrintResult();




        }

    }


    class FinalExam : Exam
    {
        public FinalExam(int time, QuestionList qList) : base(time, qList) { }

        public override void ConductExam()
        {
            Console.WriteLine("==== Final Exam ====");
            foreach (var q in Questions.GetAll())
            {
                q.Show();
                Console.Write("Your Answer (comma for multiple): ");
                string ans = Console.ReadLine();

                if (CheckAnswer(q, ans))
                    Score += q.Marks;
                Console.WriteLine();

            }

            PrintResult();




        }

    }



    // ===================== Subject =====================
    class Subject
    {
        public string Name { 
            get; set; 

        }

        public Subject(string name) { 
            Name = name; 
        }

    }








    // ===================== Program (Main) =====================
    class Program
    {
        static void Main(string[] args)
        {
            Console.WriteLine("Enter your name: ");
            string name = Console.ReadLine();
            Console.WriteLine("Enter your ID: ");
            int id = int.Parse(Console.ReadLine());


            Student student = new Student(name, id);
            StudentInfoPrinter.Print(student);


            Subject subject = new Subject("Programming & Math");


            // Practice Questions (easier)
            QuestionList practiceQ = new QuestionList("practice.log");
            practiceQ.Add(new TrueFalseQuestion("Stack is a data structure?", 2, true));
            practiceQ.Add(new ChooseOneQuestion("Which is an even number?", 2, new string[] { "3", "6", "9" }, 1));
            practiceQ.Add(new TrueFalseQuestion("Is Django a frontend framework?", 2, false));


            // Final Questions (more + harder)
            QuestionList finalQ = new QuestionList("final.log");
            finalQ.Add(new ChooseAllQuestion("Which of the following are prime numbers?", 3, new string[] { "2", "4", "5", "6" }, new int[] { 0, 2 }));
            finalQ.Add(new ChooseOneQuestion("Which of these is NOT an STL container?", 3, new string[] { "Vector", "Map", "Elephant" }, 2));
            finalQ.Add(new ChooseOneQuestion("Which of these is a backend framework?", 2, new string[] { "React", "Angular", "Django" }, 2));
            finalQ.Add(new ChooseAllQuestion("Which are mobile frameworks?", 3, new string[] { "Flutter", "React Native", "Angular", "Xamarin" }, new int[] { 0, 1, 3 }));
            finalQ.Add(new TrueFalseQuestion("Is .NET a framework?", 2, true));


            Console.WriteLine("Choose Exam Type: 1) Practice  2) Final");
            int choice = int.Parse(Console.ReadLine());


            Exam exam = choice == 1
                ? new PracticeExam(30, practiceQ)
                : new FinalExam(60, finalQ);


            exam.ConductExam();

        }

    }

}


/////////////////////-------"mini project is done"-------\\\\\\\\\\\\\\\\\\\  