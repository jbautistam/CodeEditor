using System;
using System.Windows;
using System.Windows.Controls;

using ICSharpCode.AvalonEdit.Highlighting;

namespace Bau.Controls.CodeEditor
{
	/// <summary>
	///		Control de usuario para el editor
	/// </summary>
	public partial class ctlEditor : UserControl
	{
		// Eventos públicos
		public event EventHandler TextChanged;
		//public event EventHandler<DragEventArgs> DragEnter;
		//public event EventHandler<DragEventArgs> Drop;
		// Variables privadas
		private string fileName;

		public ctlEditor()
		{
			InitializeComponent();
		}

		/// <summary>
		///		Cambia el modo de resalte a partir del nombre de archivo
		/// </summary>
		private void ChangeHighLight()
		{
			string extension = ".cs";

				// Obtiene la extensión de archivo
				if (!string.IsNullOrEmpty(FileName))
					extension = System.IO.Path.GetExtension(FileName);
				// Cambia el modo de resalte del archivo
				ChangeHighLightByExtension(extension);
		}

		/// <summary>
		///		Cambia el modo de resalte a partir del nombre de archivo
		/// </summary>
		public void ChangeHighLightByExtension(string extension)
		{
			if (!string.IsNullOrEmpty(extension))
			{ 
				// Quita los espacios
				extension = extension.Trim();
				// Añade el punto a la extensión
				if (!extension.StartsWith("."))
					extension = "." + extension;
				// Cambia el modo de resalte
				if (extension.Equals(".sql", StringComparison.CurrentCultureIgnoreCase))
					ChangeHighlightByResource("pack://application:,,,/CodeEditor;component/Resources/SqlHighLight.xml");
				else
					txtEditor.SyntaxHighlighting = HighlightingManager.Instance.GetDefinitionByExtension(extension);
			}
		}

		/// <summary>
		///		Cambia el modo de resalte de un recurso
		/// </summary>
		public bool ChangeHighlightByResource(string resource)
		{
			bool changed = false;

				// Carga el recurso
				try
				{
					using (System.IO.Stream stm = Application.GetResourceStream(new Uri(resource)).Stream)
					{
						LoadHighLight(stm);
					}
				} catch (Exception exception)
				{
					System.Diagnostics.Debug.WriteLine($"Error al cambiar el modo de resalte. {exception.Message}");
				}
				// Devuelve el valor que indica que se ha cargado
				return changed;
		}

		/// <summary>
		///		Cambia el modo de resalte desde un stream
		/// </summary>
		public void LoadHighLight(System.IO.Stream stmFile)
		{
			using (System.Xml.XmlTextReader rdrXml = new System.Xml.XmlTextReader(stmFile))
			{   
				// Cambia el modo de resalte
				txtEditor.SyntaxHighlighting = ICSharpCode.AvalonEdit.Highlighting.Xshd.HighlightingLoader.Load(rdrXml,
																												HighlightingManager.Instance);
				// Cierra el lector
				rdrXml.Close();
			}
		}

		/// <summary>
		///		Inserta un texto en el documento
		/// </summary>
		public void InsertText(string text)
		{
			txtEditor.Document.Insert(txtEditor.CaretOffset, text);
		}

		/// <summary>
		///		Texto
		/// </summary>
		public string Text
		{
			get { return txtEditor.Text; }
			set { txtEditor.Text = value; }
		}

		/// <summary>
		///		Nombre del archivo
		/// </summary>
		public string FileName
		{
			get { return fileName; }
			set
			{
				fileName = value;
				ChangeHighLight();
			}
		}

		private void txtEditor_TextChanged(object sender, EventArgs e)
		{
			TextChanged?.Invoke(this, EventArgs.Empty);
		}

		private void txtEditor_DragEnter(object sender, DragEventArgs e)
		{
			OnDragEnter(e);
		}

		private void txtEditor_Drop(object sender, DragEventArgs e)
		{
			OnDrop(e);
		}
	}
}
