
namespace MathTutor_App_
{
    public partial class MainForm : Form
    {
        public MainForm()
        { 
            InitializeComponent();

            var _trainer = new FormulaTrainer();

            /// загрузка формул из файла
            _trainer.LoadFormulaFromFile(@"input-files/Formulas.txt");


        }
    }
}
