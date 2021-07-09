using System;
using System.Windows.Forms;

namespace Congo.WFA {
	public partial class Form1 : Form {
		public Form1() {
			InitializeComponent();
		}

		private void Form1_Load(object sender, EventArgs e) {
			initHIvsHIRadioButton.Checked = true;
		}

		private void initStartButton_Click(object sender, EventArgs e) {
			if (initHIvsHIRadioButton.Checked) {

			}
			else if (initHIvsAIWhiteRadioButton.Checked) {

			}
			else if (initHIvsAIBlackRadioButton.Checked) {

			}
			else if (initAIvsAIRadioButton.Checked) {

			}
			else {
				throw new Exception();
			}
		}
	}
}
