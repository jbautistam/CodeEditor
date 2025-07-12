using System.Text.RegularExpressions;
using System.Windows;
using System.Windows.Controls;

using ICSharpCode.AvalonEdit.Document;
using ICSharpCode.AvalonEdit.Highlighting;

namespace Bau.Controls.CodeEditor;

/// <summary>
///		Control de usuario para el editor
/// </summary>
public partial class ctlEditor : UserControl
{
	// Eventos públicos
	public event EventHandler? TextChanged;
	public event EventHandler? PositionChanged;
	// Variables privadas
	private string? fileName;
	private ICSharpCode.AvalonEdit.TextEditor _selectedEditor;

	public ctlEditor()
	{
		// Inicializa los componentes
		InitializeComponent();
		// Inicialia los manejadores de eventos
		txtEditor.TextArea.Caret.PositionChanged += (sender, args) => PositionChanged?.Invoke(this, args);
		txtEditorSecondary.TextArea.Caret.PositionChanged += (sender, args) => PositionChanged?.Invoke(this, args);
		// Inicializa el editor secundario
		txtEditorSecondary.Document = txtEditor.Document;
		_selectedEditor = txtEditor;
		// Inicializa los controles
		ShowSearch(false);
		SplitDocument(false, false);
	}

	/// <summary>
	///		Divide la pantalla con dos cuadros de edición
	/// </summary>
	private void SplitDocument(bool split, bool isVertical)
	{
		// Muestra / oculta el cuadro de texto secundario
		if (split)
		{
			// Cambia la visibilidad
			txtEditorSecondary.Visibility = Visibility.Visible;
			// Coloca los editores
			if (isVertical)
			{
				// El editor principal está en la fila superior
				Grid.SetRow(txtEditor, 0);
				Grid.SetColumn(txtEditor, 0);
				Grid.SetRowSpan(txtEditor, 1);
				Grid.SetColumnSpan(txtEditor, 2);
				// El editor secundario está en la fila inferior
				Grid.SetRow(txtEditorSecondary, 1);
				Grid.SetColumn(txtEditorSecondary, 0);
				Grid.SetRowSpan(txtEditorSecondary, 1);
				Grid.SetColumnSpan(txtEditorSecondary, 2);
				// Muestra el spliter horizontal
				splitterHorizontal.Visibility = Visibility.Visible;
			}
			else
			{
				// Cambia el margen
				txtEditor.Margin = new Thickness(0, 0, 10, 0);
				// El editor principal está en la columna izquierda
				Grid.SetRow(txtEditor, 0);
				Grid.SetColumn(txtEditor, 0);
				Grid.SetRowSpan(txtEditor, 2);
				Grid.SetColumnSpan(txtEditor, 1);
				// El editor secundario está en la columna derecha
				Grid.SetRow(txtEditorSecondary, 0);
				Grid.SetColumn(txtEditorSecondary, 1);
				Grid.SetRowSpan(txtEditorSecondary, 2);
				Grid.SetColumnSpan(txtEditorSecondary, 1);
				// Muestra el spliter vertical
				splitterVertical.Visibility = Visibility.Visible;
			}
		}
		else
		{
			// Cambia el margen
			txtEditor.Margin = new Thickness(0, 0, 0, 0);
			// Cambia la visibilidad
			txtEditorSecondary.Visibility = Visibility.Collapsed;
			splitterHorizontal.Visibility = Visibility.Collapsed;
			splitterVertical.Visibility = Visibility.Collapsed;
			// El editor principal lo ocupa todo
			Grid.SetRowSpan(txtEditor, 2);
			Grid.SetColumnSpan(txtEditor, 2);
			// y es el seleccionado
			_selectedEditor = txtEditor;
		}
		// Activa / desactiva las opciones de menú
		mnuSplitHorizontal.IsEnabled = !split;
		mnuSplitVertical.IsEnabled = !split;
		mnuUnsplit.IsEnabled = split;
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
			else if (extension.Equals(".py", StringComparison.CurrentCultureIgnoreCase) ||
					 extension.Equals(".pyw", StringComparison.CurrentCultureIgnoreCase))
				ChangeHighlightByResource("pack://application:,,,/CodeEditor;component/Resources/PythonHighLight.xml");
			else
				txtEditor.SyntaxHighlighting = HighlightingManager.Instance.GetDefinitionByExtension(extension);
			// Si no se ha establecido ningún resalte, se obtiene el predeterminado
			if (txtEditor.SyntaxHighlighting == null)
				txtEditor.SyntaxHighlighting = HighlightingManager.Instance.GetDefinitionByExtension(".cs");
			// Cambia el editor secundario
			txtEditorSecondary.SyntaxHighlighting = txtEditor.SyntaxHighlighting;
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
			} 
			catch (Exception exception)
			{
				System.Diagnostics.Debug.WriteLine($"Error al cambiar el modo de resalte. {exception.Message}");
			}
			// Devuelve el valor que indica que se ha cargado
			return changed;
	}

	/// <summary>
	///		Coloca el editor en una línea
	/// </summary>
	public void GoToLine(int line, string? selectedText = null)
	{
		if (line >= 1)
		{
			DocumentLine documentLine = txtEditor.Document.GetLineByNumber(line);

				// Selecciona la línea
				txtEditor.Select(documentLine.Offset, documentLine.Length);
				// y desplaza el scroll
				txtEditor.ScrollToLine(line);
				// Si hay un texto seleccionado, selecciona ese texto
				if (!string.IsNullOrWhiteSpace(selectedText))
				{
					string lineText = txtEditor.Document.GetText(documentLine.Offset, documentLine.Length);

						if (!string.IsNullOrWhiteSpace(lineText))
						{
							int index = lineText.IndexOf(selectedText);

								if (index >= 0)
									txtEditor.Select(documentLine.Offset + index, selectedText.Length);
						}
				}
		}
	}

	/// <summary>
	///		Busca la siguiente coincidencia con el texto
	/// </summary>
	private void SearchNext(bool upToDown)
	{
		if (!string.IsNullOrWhiteSpace(txtSearch.Text))
			SearchNext(txtSearch.Text, upToDown, chkCaseSensitive.IsChecked ?? false, chkWholeWord.IsChecked ?? false, chkUseRegex.IsChecked ?? false, true);
	}

	/// <summary>
	///		Busca la siguiente coincidencia con el texto
	/// </summary>
	private bool SearchNext(string textToFind, bool upToDown, bool caseSensitive, bool wholeWord, bool useRegex, bool useWildcards)
    {
        Regex regex = GetRegEx(textToFind, upToDown, caseSensitive, wholeWord, useRegex, useWildcards);
        int start = regex.Options.HasFlag(RegexOptions.RightToLeft) ? txtEditor.SelectionStart : txtEditor.SelectionStart + txtEditor.SelectionLength;
        Match match = regex.Match(txtEditor.Text, start);

			// Si no se ha encontrado nada, comienza de nuevo por el inicio o el final
			// Si se ha encontrado algo, lo selecciona
			if (!match.Success)
			{
				if (regex.Options.HasFlag(RegexOptions.RightToLeft))
					match = regex.Match(txtEditor.Text, txtEditor.Text.Length);
				else
					match = regex.Match(txtEditor.Text, 0);
			}
			else
			{
				TextLocation loc = txtEditor.Document.GetLocation(match.Index);

					// Selecciona el texto
					txtEditor.Select(match.Index, match.Length);
					// Posiciona el control
					txtEditor.ScrollTo(loc.Line, loc.Column);
			}
			// Devuelve el valor que indica si se ha encontrado algo más
			return match.Success;
	}

	/// <summary>
	///		Obtiene la expresión regular
	/// </summary>
    private Regex GetRegEx(string textToFind, bool upToDown, bool caseSensitive, bool wholeWord, bool useRegex, bool useWildcards, bool leftToRight = false)
    {
        RegexOptions options = RegexOptions.None;

			// Añade las opciones de búsqueda hacia atrás y distinguiendo mayúsculas y minúsculas
			if (!upToDown && !leftToRight)
				options |= RegexOptions.RightToLeft;
			if (!caseSensitive)
				options |= RegexOptions.IgnoreCase;
			// Obtiene el patrón
			if (useRegex)
				return new Regex(textToFind, options);
			else
			{
				string pattern = Regex.Escape(textToFind);

					// Sustituye los comodines por caracteres de expresiones regulares
					if (useWildcards)
						pattern = pattern.Replace("\\*", ".*").Replace("\\?", ".");
					// Añade los patrones de búsqueda por palabra completa
					if (wholeWord)
						pattern = "\\b" + pattern + "\\b";
					// Devuelve la expresión regular
					return new Regex(pattern, options);
			}
	}

	/// <summary>
	///		Reemplaza el texto
	/// </summary>
	private void Replace(bool replaceAll)
	{
		if (!string.IsNullOrWhiteSpace(txtSearch.Text))
		{
			if (replaceAll)
				ReplaceAll(txtSearch.Text, txtReplace.Text, chkCaseSensitive.IsChecked ?? false, chkWholeWord.IsChecked ?? false, chkUseRegex.IsChecked ?? false, true);
			else
			{
				// Reemplaza el texto
				Replace(txtSearch.Text, txtReplace.Text, chkCaseSensitive.IsChecked ?? false, chkWholeWord.IsChecked ?? false, chkUseRegex.IsChecked ?? false, true);
				// Pasa a la siguiente búsqueda
				SearchNext(true);
			}
		}
	}

	/// <summary>
	///		Reemplaza el texto
	/// </summary>
    private bool Replace(string textToFind, string textToReplace, bool caseSensitive, bool wholeWord, bool useRegex, bool useWildcards)
    {
        Regex regex = GetRegEx(textToFind, true, caseSensitive, wholeWord, useRegex, useWildcards);
        string input = txtEditor.Text.Substring(txtEditor.SelectionStart, txtEditor.SelectionLength);
        Match match = regex.Match(input);
        bool replaced = false;

			// Ajusta el texto a reemplazar para evitar los nulos
			if (textToReplace is null)
				textToReplace = "";
			// Reemplaza el texto si lo encuentra
			if (match.Success && match.Index == 0 && match.Length == input.Length)
			{
				txtEditor.Document.Replace(txtEditor.SelectionStart, txtEditor.SelectionLength, textToReplace);
				replaced = true;
			}
			// Devuelve el valor que indica si se ha reemplazado algo
			return replaced;
	}

	/// <summary>
	///		Reemplaza todas las coincidencias
	/// </summary>
    private void ReplaceAll(string textToFind, string textToReplace, bool caseSensitive, bool wholeWord, bool useRegex, bool useWildcards)
    {
        Regex regex = GetRegEx(textToFind, true, caseSensitive, wholeWord, useRegex, useWildcards);
        int offset = 0;

			// Ajusta el texto a reemplazar para evitar los nulos
			if (textToReplace is null)
				textToReplace = "";
			// Comienza los cambios
            txtEditor.BeginChange();
			// Por cada coincidencia
			foreach (Match match in regex.Matches(txtEditor.Text))
				if (offset < txtEditor.Document.TextLength)
					try
					{
						// Reemplaza el texto
						txtEditor.Document.Replace(offset + match.Index, match.Length, textToReplace);
						// Pasa al siguiente
						offset += match.Index + textToReplace.Length;
					}
					catch (Exception exception)
					{
						System.Diagnostics.Trace.TraceError(exception.Message);
					}
			// Finaliza los cambios
            txtEditor.EndChange();
    }

	/// <summary>
	///		Cambia el modo de resalte desde un stream
	/// </summary>
	public void LoadHighLight(System.IO.Stream stmFile)
	{
		using (System.Xml.XmlTextReader rdrXml = new System.Xml.XmlTextReader(stmFile))
		{   
			// Cambia el modo de resalte
			txtEditor.SyntaxHighlighting = ICSharpCode.AvalonEdit.Highlighting.Xshd.HighlightingLoader.Load(rdrXml, HighlightingManager.Instance);
			txtEditorSecondary.SyntaxHighlighting = txtEditor.SyntaxHighlighting;
			// Cierra el lector
			rdrXml.Close();
		}
	}

	/// <summary>
	///		Obtiene el texto seleccionado en el editor
	/// </summary>
	public string GetSelectedText() => _selectedEditor.SelectedText;

	/// <summary>
	///		Inserta un texto en el documento en la posición del ratón
	/// </summary>
	public void InsertText(string text, Point mousePoint)
	{
		InsertText(text);
	}

	/// <summary>
	///		Inserta un texto en el documento
	/// </summary>
	public void InsertText(string text, int offset = 0)
	{
		_selectedEditor.Document.Insert(_selectedEditor.CaretOffset, text);
		if (offset < 0)
			_selectedEditor.CaretOffset += offset;
		_selectedEditor.Focus();
	}

	/// <summary>
	///		Abre / cierra el control de búsqueda
	/// </summary>
	public void ShowSearch(bool open)
	{
		if (open)
		{
			grdSearch.Visibility = Visibility.Visible;
			txtSearch.Text = GetSearchText(GetSelectedText());
			txtSearch.Focus();
		}
		else
		{
			grdSearch.Visibility = Visibility.Hidden;
			_selectedEditor.Focus();
		}

		// Obtiene el texto de búsqueda, quita los saltos de línea y demás
		string GetSearchText(string text)
		{
			// Quita los saltos de línea
			while (!string.IsNullOrWhiteSpace(text) && text.IndexOfAny(new [] {'\r', '\n'}) >= 0)
				text = text.Replace('\r', ' ').Replace('\n', ' ');
			// Devuelve el texto
			return text;
		}
	}

	/// <summary>
	///		Texto
	/// </summary>
	public string Text
	{
		get { return txtEditor.Text; }
		set 
		{ 
			if (!txtEditor.Text.Equals(value, StringComparison.CurrentCultureIgnoreCase))
				 txtEditor.Text = value; 
		}
	}

	/// <summary>
	///		Indica si el editor está en modo de sólo lectura
	/// </summary>
	public bool ReadOnly
	{
		get { return txtEditor.IsReadOnly; }
		set 
		{ 
			txtEditor.IsReadOnly = value; 
			txtEditorSecondary.IsReadOnly = value;
		}
	}

	/// <summary>
	///		Nombre del archivo
	/// </summary>
	public string? FileName
	{
		get { return fileName; }
		set
		{
			fileName = value;
			ChangeHighLight();
		}
	}

	/// <summary>
	///		Nombre de la fuente
	/// </summary>
	public string EditorFontName
	{
		get { return txtEditor.FontFamily.Source; }
		set 
		{
			if (!string.IsNullOrWhiteSpace(value))
			try
			{
				txtEditor.FontFamily = new System.Windows.Media.FontFamily(value);
				txtEditorSecondary.FontFamily = txtEditor.FontFamily;
			}
			catch (Exception exception)
			{
				System.Diagnostics.Debug.WriteLine($"Error when set font {exception.Message}");
			}
		}
	}

	/// <summary>
	///		Tamaño de fuente
	/// </summary>
	public double EditorFontSize
	{
		get { return txtEditor.FontSize; }
		set 
		{ 
			txtEditor.FontSize = value; 
			txtEditorSecondary.FontSize = value;
		}
	}

	/// <summary>
	///		Indica si se debe mostrar el número de línea
	/// </summary>
	public bool ShowLinesNumber
	{
		get { return txtEditor.ShowLineNumbers; }
		set 
		{ 
			txtEditor.ShowLineNumbers = value; 
			txtEditorSecondary.ShowLineNumbers = value;
		}
	}

	/// <summary>
	///		Línea del cursor
	/// </summary>
	public int Line
	{
		get { return txtEditor.TextArea.Caret.Line; }
		set { txtEditor.TextArea.Caret.Line = value; }
	}

	/// <summary>
	///		Columna del cursor
	/// </summary>
	public int Column
	{
		get { return txtEditor.TextArea.Caret.Column; }
		set { txtEditor.TextArea.Caret.Column = value; }
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

	private void mnuSplitHorizontal_Click(object sender, RoutedEventArgs e)
	{
		SplitDocument(true, false);
	}

	private void mnuSplitVertical_Click(object sender, RoutedEventArgs e)
	{
		SplitDocument(true, true);
	}

	private void mnuUnsplit_Click(object sender, RoutedEventArgs e)
	{
		SplitDocument(false, false);
	}

	private void splitter_MouseDoubleClick(object sender, System.Windows.Input.MouseButtonEventArgs e)
	{
		SplitDocument(false, false);
	}

	private void txtEditor_GotFocus(object sender, RoutedEventArgs e)
	{
		_selectedEditor = txtEditor;
	}

	private void txtEditorSecondary_GotFocus(object sender, RoutedEventArgs e)
	{
		_selectedEditor = txtEditorSecondary;
	}

	private void cmdCloseSearch_Click(object sender, RoutedEventArgs e)
	{
		ShowSearch(false);
	}

	private void cmdSearch_Click(object sender, RoutedEventArgs e)
	{
		SearchNext(true);
	}

	private void cmdReplaceAll_Click(object sender, RoutedEventArgs e)
	{
		Replace(true);
	}

	private void cmdReplaceNext_Click(object sender, RoutedEventArgs e)
	{
		Replace(false);
	}

	private void cmdSearchPrevious_Click(object sender, RoutedEventArgs e)
	{
		SearchNext(false);
	}

	private void cmdOpenSearch_Click(object sender, RoutedEventArgs e)
	{
		ShowSearch(true);
    }
}
