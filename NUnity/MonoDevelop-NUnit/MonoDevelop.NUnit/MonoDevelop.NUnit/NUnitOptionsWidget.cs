using GLib;
using Gtk;
using Mono.Unix;
using MonoDevelop.Core;
using Stetic;
using System;
namespace MonoDevelop.NUnit
{
	internal class NUnitOptionsWidget : Bin
	{
		private TreeStore store;
		private TreeViewColumn textColumn;
		private UnitTest test;
		private string config;
		private NUnitCategoryOptions options;
		private NUnitCategoryOptions localOptions;
		private VBox vbox1;
		private CheckButton useParentCheck;
		private HSeparator hseparator1;
		private VBox vbox3;
		private Label label1;
		private HBox hbox2;
		private Label label2;
		private VBox vbox4;
		private RadioButton noFilterRadio;
		private RadioButton includeRadio;
		private RadioButton excludeRadio;
		private Label label3;
		private HBox hbox1;
		private Label label4;
		private ScrolledWindow scrolledwindow1;
		private TreeView categoryTree;
		private VBox vbox2;
		private Button addButton;
		private Button removeButton;
		public NUnitOptionsWidget(Properties customizationObject)
		{
			this.Build();
			this.test = customizationObject.Get<UnitTest>("UnitTest");
			this.config = customizationObject.Get<string>("Config");
			this.options = (this.localOptions = (NUnitCategoryOptions)this.test.GetOptions(typeof(NUnitCategoryOptions), this.config));
			this.store = new TreeStore(new System.Type[]
			{
				typeof(string)
			});
			this.categoryTree.set_Model(this.store);
			this.categoryTree.set_HeadersVisible(false);
			CellRendererText tr = new CellRendererText();
			tr.set_Editable(true);
			tr.add_Edited(new EditedHandler(this.OnCategoryEdited));
			this.textColumn = new TreeViewColumn();
			this.textColumn.set_Title(GettextCatalog.GetString("Category"));
			this.textColumn.PackStart(tr, false);
			this.textColumn.AddAttribute(tr, "text", 0);
			this.textColumn.set_Expand(true);
			this.categoryTree.AppendColumn(this.textColumn);
			if (this.test.Parent != null)
			{
				this.useParentCheck.set_Active(!this.test.HasOptions(typeof(NUnitCategoryOptions), this.config));
			}
			else
			{
				this.useParentCheck.set_Active(false);
				this.useParentCheck.set_Sensitive(false);
			}
			if (!this.options.EnableFilter)
			{
				this.noFilterRadio.set_Active(true);
			}
			else
			{
				if (this.options.Exclude)
				{
					this.excludeRadio.set_Active(true);
				}
				else
				{
					this.includeRadio.set_Active(true);
				}
			}
			this.Fill();
			this.noFilterRadio.add_Toggled(new System.EventHandler(this.OnFilterToggled));
			this.includeRadio.add_Toggled(new System.EventHandler(this.OnFilterToggled));
			this.excludeRadio.add_Toggled(new System.EventHandler(this.OnFilterToggled));
			this.useParentCheck.add_Toggled(new System.EventHandler(this.OnToggledUseParent));
			this.addButton.add_Clicked(new System.EventHandler(this.OnAddCategory));
			this.removeButton.add_Clicked(new System.EventHandler(this.OnRemoveCategory));
		}
		private void Fill()
		{
			this.noFilterRadio.set_Sensitive(!this.useParentCheck.get_Active());
			this.includeRadio.set_Sensitive(!this.useParentCheck.get_Active());
			this.excludeRadio.set_Sensitive(!this.useParentCheck.get_Active());
			this.categoryTree.set_Sensitive(!this.useParentCheck.get_Active() && !this.noFilterRadio.get_Active());
			this.removeButton.set_Sensitive(!this.useParentCheck.get_Active() && !this.noFilterRadio.get_Active());
			this.addButton.set_Sensitive(!this.useParentCheck.get_Active() && !this.noFilterRadio.get_Active());
			this.store.Clear();
			foreach (string cat in this.options.Categories)
			{
				this.store.AppendValues(new object[]
				{
					cat
				});
			}
		}
		private void OnToggledUseParent(object sender, System.EventArgs args)
		{
			if (this.useParentCheck.get_Active())
			{
				this.options = (NUnitCategoryOptions)this.test.Parent.GetOptions(typeof(NUnitCategoryOptions), this.config);
			}
			else
			{
				this.options = this.localOptions;
			}
			if (!this.options.EnableFilter)
			{
				this.noFilterRadio.set_Active(true);
			}
			else
			{
				if (this.options.Exclude)
				{
					this.excludeRadio.set_Active(true);
				}
				else
				{
					this.includeRadio.set_Active(true);
				}
			}
			this.Fill();
		}
		private void OnFilterToggled(object sender, System.EventArgs args)
		{
			this.options.EnableFilter = !this.noFilterRadio.get_Active();
			this.options.Exclude = this.excludeRadio.get_Active();
			this.Fill();
		}
		private void OnAddCategory(object sender, System.EventArgs args)
		{
			TreeIter it = this.store.AppendValues(new object[]
			{
				""
			});
			this.categoryTree.SetCursor(this.store.GetPath(it), this.textColumn, true);
		}
		private void OnRemoveCategory(object sender, System.EventArgs args)
		{
			TreeModel foo;
			TreeIter iter;
			if (this.categoryTree.get_Selection().GetSelected(ref foo, ref iter))
			{
				string old = (string)this.store.GetValue(iter, 0);
				this.options.Categories.Remove(old);
				this.store.Remove(ref iter);
			}
		}
		private void OnCategoryEdited(object sender, EditedArgs args)
		{
			TreeIter iter;
			if (this.store.GetIter(ref iter, new TreePath(args.get_Path())))
			{
				string old = (string)this.store.GetValue(iter, 0);
				if (args.get_NewText().Length == 0)
				{
					this.options.Categories.Remove(old);
					this.store.Remove(ref iter);
				}
				else
				{
					int i = this.options.Categories.IndexOf(old);
					if (i == -1)
					{
						this.options.Categories.Add(args.get_NewText());
					}
					else
					{
						this.options.Categories[i] = args.get_NewText();
					}
					this.store.SetValue(iter, 0, args.get_NewText());
				}
			}
		}
		public void Store(Properties customizationObject)
		{
			if (this.useParentCheck.get_Active())
			{
				this.test.ResetOptions(typeof(NUnitCategoryOptions), this.config);
			}
			else
			{
				this.test.SetOptions(this.options, this.config);
			}
		}
		protected virtual void Build()
		{
			Gui.Initialize(this);
			BinContainer.Attach(this);
			base.set_Name("MonoDevelop.NUnit.NUnitOptionsWidget");
			this.vbox1 = new VBox();
			this.vbox1.set_Name("vbox1");
			this.vbox1.set_Spacing(6);
			this.useParentCheck = new CheckButton();
			this.useParentCheck.set_Name("useParentCheck");
			this.useParentCheck.set_Label(Catalog.GetString("Use parent test settings"));
			this.useParentCheck.set_DrawIndicator(true);
			this.useParentCheck.set_UseUnderline(true);
			this.vbox1.Add(this.useParentCheck);
			Box.BoxChild w = (Box.BoxChild)this.vbox1.get_Item(this.useParentCheck);
			w.set_Position(0);
			w.set_Expand(false);
			w.set_Fill(false);
			this.hseparator1 = new HSeparator();
			this.hseparator1.set_Name("hseparator1");
			this.vbox1.Add(this.hseparator1);
			Box.BoxChild w2 = (Box.BoxChild)this.vbox1.get_Item(this.hseparator1);
			w2.set_Position(1);
			w2.set_Expand(false);
			w2.set_Fill(false);
			this.vbox3 = new VBox();
			this.vbox3.set_Name("vbox3");
			this.vbox3.set_Spacing(6);
			this.label1 = new Label();
			this.label1.set_Name("label1");
			this.label1.set_Xalign(0f);
			this.label1.set_LabelProp(Catalog.GetString("The following filter will be applied when running the tests:"));
			this.vbox3.Add(this.label1);
			Box.BoxChild w3 = (Box.BoxChild)this.vbox3.get_Item(this.label1);
			w3.set_Position(0);
			w3.set_Expand(false);
			w3.set_Fill(false);
			this.hbox2 = new HBox();
			this.hbox2.set_Name("hbox2");
			this.label2 = new Label();
			this.label2.set_WidthRequest(18);
			this.label2.set_Name("label2");
			this.hbox2.Add(this.label2);
			Box.BoxChild w4 = (Box.BoxChild)this.hbox2.get_Item(this.label2);
			w4.set_Position(0);
			w4.set_Expand(false);
			w4.set_Fill(false);
			this.vbox4 = new VBox();
			this.vbox4.set_Name("vbox4");
			this.vbox4.set_Spacing(6);
			this.noFilterRadio = new RadioButton(Catalog.GetString("Don't apply any filter"));
			this.noFilterRadio.set_Name("noFilterRadio");
			this.noFilterRadio.set_Active(true);
			this.noFilterRadio.set_DrawIndicator(true);
			this.noFilterRadio.set_UseUnderline(true);
			this.noFilterRadio.set_Group(new SList(System.IntPtr.Zero));
			this.vbox4.Add(this.noFilterRadio);
			Box.BoxChild w5 = (Box.BoxChild)this.vbox4.get_Item(this.noFilterRadio);
			w5.set_Position(0);
			w5.set_Expand(false);
			w5.set_Fill(false);
			this.includeRadio = new RadioButton(Catalog.GetString("Include the following categories"));
			this.includeRadio.set_Name("includeRadio");
			this.includeRadio.set_DrawIndicator(true);
			this.includeRadio.set_UseUnderline(true);
			this.includeRadio.set_Group(this.noFilterRadio.get_Group());
			this.vbox4.Add(this.includeRadio);
			Box.BoxChild w6 = (Box.BoxChild)this.vbox4.get_Item(this.includeRadio);
			w6.set_Position(1);
			w6.set_Expand(false);
			w6.set_Fill(false);
			this.excludeRadio = new RadioButton(Catalog.GetString("Exclude the following categories"));
			this.excludeRadio.set_Name("excludeRadio");
			this.excludeRadio.set_DrawIndicator(true);
			this.excludeRadio.set_UseUnderline(true);
			this.excludeRadio.set_Group(this.noFilterRadio.get_Group());
			this.vbox4.Add(this.excludeRadio);
			Box.BoxChild w7 = (Box.BoxChild)this.vbox4.get_Item(this.excludeRadio);
			w7.set_Position(2);
			w7.set_Expand(false);
			w7.set_Fill(false);
			this.hbox2.Add(this.vbox4);
			Box.BoxChild w8 = (Box.BoxChild)this.hbox2.get_Item(this.vbox4);
			w8.set_Position(1);
			this.vbox3.Add(this.hbox2);
			Box.BoxChild w9 = (Box.BoxChild)this.vbox3.get_Item(this.hbox2);
			w9.set_Position(1);
			this.vbox1.Add(this.vbox3);
			Box.BoxChild w10 = (Box.BoxChild)this.vbox1.get_Item(this.vbox3);
			w10.set_Position(2);
			w10.set_Expand(false);
			w10.set_Fill(false);
			this.label3 = new Label();
			this.label3.set_Name("label3");
			this.label3.set_Xalign(0f);
			this.label3.set_LabelProp(Catalog.GetString("Categories:"));
			this.vbox1.Add(this.label3);
			Box.BoxChild w11 = (Box.BoxChild)this.vbox1.get_Item(this.label3);
			w11.set_Position(3);
			w11.set_Expand(false);
			w11.set_Fill(false);
			this.hbox1 = new HBox();
			this.hbox1.set_Name("hbox1");
			this.hbox1.set_Spacing(6);
			this.label4 = new Label();
			this.label4.set_WidthRequest(18);
			this.label4.set_Name("label4");
			this.hbox1.Add(this.label4);
			Box.BoxChild w12 = (Box.BoxChild)this.hbox1.get_Item(this.label4);
			w12.set_Position(0);
			w12.set_Expand(false);
			w12.set_Fill(false);
			this.scrolledwindow1 = new ScrolledWindow();
			this.scrolledwindow1.set_Name("scrolledwindow1");
			this.scrolledwindow1.set_ShadowType(1);
			this.categoryTree = new TreeView();
			this.categoryTree.set_Name("categoryTree");
			this.scrolledwindow1.Add(this.categoryTree);
			this.hbox1.Add(this.scrolledwindow1);
			Box.BoxChild w13 = (Box.BoxChild)this.hbox1.get_Item(this.scrolledwindow1);
			w13.set_Position(1);
			this.vbox2 = new VBox();
			this.vbox2.set_Name("vbox2");
			this.vbox2.set_Spacing(6);
			this.addButton = new Button();
			this.addButton.set_Name("addButton");
			this.addButton.set_UseStock(true);
			this.addButton.set_UseUnderline(true);
			this.addButton.set_Label("gtk-add");
			this.vbox2.Add(this.addButton);
			Box.BoxChild w14 = (Box.BoxChild)this.vbox2.get_Item(this.addButton);
			w14.set_Position(0);
			w14.set_Expand(false);
			w14.set_Fill(false);
			this.removeButton = new Button();
			this.removeButton.set_Name("removeButton");
			this.removeButton.set_UseStock(true);
			this.removeButton.set_UseUnderline(true);
			this.removeButton.set_Label("gtk-remove");
			this.vbox2.Add(this.removeButton);
			Box.BoxChild w15 = (Box.BoxChild)this.vbox2.get_Item(this.removeButton);
			w15.set_Position(1);
			w15.set_Expand(false);
			w15.set_Fill(false);
			this.hbox1.Add(this.vbox2);
			Box.BoxChild w16 = (Box.BoxChild)this.hbox1.get_Item(this.vbox2);
			w16.set_Position(2);
			w16.set_Expand(false);
			w16.set_Fill(false);
			this.vbox1.Add(this.hbox1);
			Box.BoxChild w17 = (Box.BoxChild)this.vbox1.get_Item(this.hbox1);
			w17.set_Position(4);
			base.Add(this.vbox1);
			if (base.get_Child() != null)
			{
				base.get_Child().ShowAll();
			}
			base.Show();
		}
	}
}
