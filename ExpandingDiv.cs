using System;
using System.Web.UI;
using System.Web.UI.WebControls;
using System.ComponentModel;
using System.Web.UI.HtmlControls;
using System.Web.UI.Design;
using System.Drawing.Design;
using System.ComponentModel.Design;

[assembly: TagPrefix("MANSoft", "MANSoft")]
namespace MANSoftLib
{
	/// <summary>
	/// Summary description for ExpandingDiv.
	/// </summary>
	[ToolboxData("<{0}:ExpandingDiv runat=server></{0}:ExpandingDiv>")]
	[ParseChildren(true)]
	[DesignerAttribute(typeof(ExpandingDivDesigner), typeof(IDesigner))]
	public class ExpandingDiv : WebControl, INamingContainer
	{
		/// <summary>
		/// Internal class for template items
		/// </summary>
		//[ParseChildren(true)]
		private sealed class ExpandingDivItem : TableRow, INamingContainer
		{
			public ExpandingDivItem()
			{
				// Create a cell to place template content into
				TableCell cell = new TableCell();
				cell.ColumnSpan = 2;
				Cells.Add(cell);
				
				// Set the initial style for this row
				Style.Add("display", "block");
			}
		}
		
		// Begin ExpandingDiv		
		private ITemplate m_ItemTemplate;
		
		private const string CONTENT_ID = "Content";
		private const string ICON_ID = "Icon";

		private string m_strExpandedIcon;
		private string m_strCollaspedIcon;
		private string m_strHeaderImage;
		private string m_strHeaderText;
		private System.Drawing.Color m_clBkColor;
		private int m_nBorder = 0;
		private int m_nCellPadding = 0;
		private int m_nCellSpacing = 0;
		private string m_strHeaderAlignment = "left";
		private string m_strHeaderStyle;

		protected override void CreateChildControls()
		{
			// Needed to have the icons specified first
			if( m_strExpandedIcon == string.Empty || m_strCollaspedIcon == string.Empty )
				throw new Exception("Expanded and collapsed icon must be defined");
			
			// Start with a clean slate
			Controls.Clear();
	
			// Create the table that will house everything
			Table table = new Table();
			table.Attributes.Add("border", Border.ToString());
			table.Attributes.Add("CellPadding", CellPadding.ToString());
			table.Attributes.Add("CellSpacing", CellSpacing.ToString());
			table.Width = Width;
			
			// Add to the controls collection now so
			// a UniqueID is generated
			Controls.Add(table);

			// Create the header row and add it to the table
			TableRow rowHeader = CreateHeaderRow();
			table.Rows.Add(rowHeader);

			// If a template has been defined
			if( m_ItemTemplate != null )
			{
				CreateTemplateRow(ref table);

				string strParams = string.Format("Expand('{0}_{1}', '{0}_{2}')", UniqueID, CONTENT_ID, ICON_ID);
				rowHeader.Cells[0].Attributes.Add("OnClick", strParams);
				rowHeader.Cells[0].Attributes.Add("OnMouseOver", "this.style.cursor='pointer'") ;
			}
			
			// Regisiter the script for client-side event
			RegisterScript();
		}
		
		/// <summary>
		/// Create the header row for this control
		/// including the images and text
		/// </summary>
		/// <returns>TableRow</returns>
		private TableRow CreateHeaderRow()
		{
			TableRow row = new TableRow();
			TableCell cell = new TableCell();

			// Add the attributes for the header row
			row.Attributes.Add("align", HeaderAlignment);
			if( HeaderCssClass != string.Empty )
				row.Attributes.Add("class", HeaderCssClass);

			// If a background color has been specified use it.
			if( m_clBkColor != System.Drawing.Color.Empty )
				row.Attributes.Add("BgColor", System.Drawing.ColorTranslator.ToHtml(m_clBkColor));

			// Create image for expand/collapse icon
			HtmlImage imgIcon = new HtmlImage();
			// Start with the table expanded			
			imgIcon.Src = ExpandedIcon;
			imgIcon.ID = ICON_ID;
			// Set the width of the cell to the size of the image
			cell.Width = GetImageWidth(ExpandedIcon);
			// Add the image to the cell
			cell.Controls.Add(imgIcon);

			// Add the cell to the row
			row.Cells.Add(cell);
			
			// Create a new cell for the header text or image
			cell = new TableCell();
			
			// If a header image has been specified
			if( HeaderImage != null )
			{
				HtmlImage img = new HtmlImage();
				img.Src = HeaderImage;
				cell.Controls.Add(img);
			}
			// otherwise use any text defined
			else if( HeaderText != string.Empty )
			{
				cell.Controls.Add(new LiteralControl(HeaderText) );
			}

			// Add the cell to the row
			row.Cells.Add(cell);

			return row;
		}
		
		/// <summary>
		/// Create row for item template and populate
		/// </summary>
		private void CreateTemplateRow(ref Table table)
		{
			ExpandingDivItem item = new ExpandingDivItem();
			m_ItemTemplate.InstantiateIn(item.Cells[0]);
			item.ID = CONTENT_ID;
			
			table.Rows.Add(item);
		}

		/// <summary>
		/// Register JavaScript necessary for controls client side
		/// actions on page
		/// </summary>
		private void RegisterScript()
		{
			System.Text.StringBuilder sb = new System.Text.StringBuilder();
			sb.Append("<script language=\"javascript\">\n");
			sb.Append("\tfunction Expand(ctrl, img)\n");
			sb.Append("\t{\n");
			sb.Append("\tvar oDiv = document.getElementById(ctrl);\n");
			sb.Append("\tvar oImg = document.getElementById(img);\n");
			sb.Append("\t\tif( oDiv.style.display == 'none' || oDiv.style.display == '')\n");
			sb.Append("\t\t{\n");
			sb.Append("\t\t\toDiv.style.display = 'block';\n");
			sb.AppendFormat("\t\t\toImg.src = '{0}';\n", ExpandedIcon);
			sb.Append("\t\t}\nelse\n\t{\n");
			sb.Append("\t\t\toDiv.style.display = 'none';\n");
			sb.AppendFormat("\t\t\toImg.src = '{0}';\n", CollapsedIcon);
			sb.Append("\t\t}\n\t}\n</script>");

			Page.RegisterClientScriptBlock("Expand", sb.ToString());
		}
		
		
		/// <summary>
		/// Retrieve the width of the designated image
		/// </summary>
		/// <param name="strImage">Image name</param>
		/// <returns>Width of image or a default value of 1</returns>
		private int GetImageWidth(string strImage)
		{
			System.Drawing.Image img = null;
			try
			{
				string strPath = Page.Server.MapPath(strImage);
				img = System.Drawing.Image.FromFile(strPath);
				if( img != null )
					return img.Width;
				else
					return 1;
			}
			catch
			{
				return 1;
			}
			finally
			{
				img.Dispose();
			}
		}
		
		#region Properties

		[TemplateContainer(typeof(ExpandingDivItem))]
		[ReadOnly(true)]
		[Bindable(false)]
		[Browsable(false)]
		public ITemplate ItemTemplate
		{
			get{ return m_ItemTemplate; }
			set{ m_ItemTemplate = value; }
		}

		[Bindable(false)]
		[Category("Header")]
		[ReadOnly(false)]
		[Description("Image to use for expanded status")]
		[Editor(typeof(ImageUrlEditor), typeof(UITypeEditor))]
		public string ExpandedIcon
		{
			get{ return m_strExpandedIcon; }
			set{ m_strExpandedIcon = value; }
		}

		[Bindable(false)]
		[Category("Header")]
		[ReadOnly(false)]
		[Description("Image to use for collasped status")]
		[Editor(typeof(ImageUrlEditor), typeof(UITypeEditor))]
		public string CollapsedIcon
		{
			get{ return m_strCollaspedIcon; }
			set{ m_strCollaspedIcon = value; }
		}

		[Bindable(false)]
		[Category("Header")]
		[ReadOnly(false)]
		[Description("Header image")]
		[Editor(typeof(ImageUrlEditor), typeof(UITypeEditor))]
		public string HeaderImage
		{
			get{ return m_strHeaderImage; }
			set{ m_strHeaderImage = value; }
		}

		[Bindable(false)]
		[Category("Header")]
		[ReadOnly(false)]
		[Description("Header text")]
		public string HeaderText
		{
			get{ return m_strHeaderText; }
			set{ m_strHeaderText = value; }
		}

		[Bindable(false)]
		[Category("Header")]
		[ReadOnly(false)]
		[Description("Header background color")]
		[Editor(typeof(ColorEditor), typeof(UITypeEditor))]
		public System.Drawing.Color HeaderBackgroundColor
		{
			get{ return m_clBkColor; }
			set{ m_clBkColor = value; }
		}
		
		[Bindable(false)]
		[Category("Appearance")]
		[ReadOnly(false)]
		[Description("Border width")]
		public int Border
		{
			get{ return m_nBorder; }
			set{ m_nBorder = value; }
		}

		[Bindable(false)]
		[Category("Appearance")]
		[ReadOnly(false)]
		[Description("Cell padding")]
		public int CellPadding
		{
			get{ return m_nCellPadding; }
			set{ m_nCellPadding = value; }
		}

		[Bindable(false)]
		[Category("Appearance")]
		[ReadOnly(false)]
		[Description("Cell spacing")]
		public int CellSpacing
		{
			get{ return m_nCellSpacing; }
			set{ m_nCellSpacing = value; }
		}

		[Bindable(false)]
		[Category("Appearance")]
		[ReadOnly(false)]
		[Description("HeaderAlignment")]
		public string HeaderAlignment
		{
			get{ return m_strHeaderAlignment; }
			set{ m_strHeaderAlignment = value; }
		}

		[Bindable(false)]
		[Category("Appearance")]
		[ReadOnly(false)]
		[Description("Header Style")]
		public string HeaderCssClass
		{
			get{ return m_strHeaderStyle; }
			set{ m_strHeaderStyle = value; }
		}

		#endregion
	}

	#region Designer
	/// <summary>
	/// Designer
	/// </summary>
	public sealed class ExpandingDivDesigner : ControlDesigner
	{
		public ExpandingDivDesigner()
		{

		}

		public override string GetDesignTimeHtml()
		{
			try
			{
				System.Text.StringBuilder sb = new System.Text.StringBuilder();

				sb.AppendFormat("<table border={0} cellpadding={1} cellspacing={2} width={3}>", ((ExpandingDiv)Component).Border, ((ExpandingDiv)Component).CellPadding, ((ExpandingDiv)Component).CellSpacing, ((ExpandingDiv)Component).Width);

				if( ((ExpandingDiv)Component).HeaderBackgroundColor != System.Drawing.Color.Empty )
					sb.AppendFormat("<tr bgcolor={0} class={1}>", System.Drawing.ColorTranslator.ToHtml(((ExpandingDiv)Component).HeaderBackgroundColor), ((ExpandingDiv)Component).HeaderCssClass);
				else
					sb.Append("<tr>");	
				
				if( ((ExpandingDiv)Component).ExpandedIcon == string.Empty )
					throw new Exception("Expanded Icon not defined");
					
				sb.AppendFormat("<td align={1} width=1><img src='{0}'></td>", ((ExpandingDiv)Component).ExpandedIcon, ((ExpandingDiv)Component).HeaderAlignment);

				if( ((ExpandingDiv)Component).HeaderImage != null )
					sb.AppendFormat("<td><img src='{0}'></td>", ((ExpandingDiv)Component).HeaderImage);
				else if( ((ExpandingDiv)Component).HeaderText != string.Empty )
					sb.AppendFormat("<td>{0}</td>", ((ExpandingDiv)Component).HeaderText );

				sb.Append("</tr>");

				if( ((ExpandingDiv)Component).ItemTemplate != null )
				{
					Literal literal = new Literal();
					((ExpandingDiv)Component).ItemTemplate.InstantiateIn(literal);
					sb.AppendFormat("<tr><td colspan=2>{0}</td></tr>", literal.Text);
				}
				sb.Append("</table>");

				return sb.ToString();	
			}
			catch(Exception ex)
			{
				return GetErrorDesignTimeHtml(ex);
			}
		}

		protected override string GetErrorDesignTimeHtml(Exception e)
		{
			return CreatePlaceHolderDesignTimeHtml(e.Message);
		}
	}

	#endregion
}
