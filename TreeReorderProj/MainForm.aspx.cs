using System;
using System.Collections.Generic;
using System.Web.UI.WebControls;
using System.Web.Services;
using System.Data;
using System.Data.SqlClient;
using System.Configuration;

namespace TreeReorderProj
{
	public partial class MainForm : System.Web.UI.Page
	{
		//Словарь для хранения состояния дерева (открытые и закрытые ноды)
		protected static Dictionary<string, bool?> treeViewState = new Dictionary<string, bool?>();

		//Строка для хранения рефрена PostBack ивента, для его последующего вызова
		protected string PostBackString = "";

		protected void Page_Load(object sender, EventArgs e)
		{
			//Сохранение рефрена PostBack ивента
			PostBackString = ClientScript.GetPostBackEventReference(this, "MyCustomArgument");

			//DataBound дерева
			GetTreeViewItems();

			if (!IsPostBack)
			{
				MyTreeView.CollapseAll();
			}
			else //Если страница загружена через PostBack - восстановить состояние нодов (открытые, звкрытые)
			{
				RestoreChildeNodeState(MyTreeView.Nodes);
			}
		}

		//Метод восстановления состояния нодов (открытыеб закрытые)
		protected void RestoreChildeNodeState(TreeNodeCollection nodes)
		{
			foreach (TreeNode node in nodes)
			{
				_ = treeViewState.TryGetValue(node.Text, out bool? boolTreeNodeFlag);

				if (boolTreeNodeFlag != null)
				{
					node.Expanded = boolTreeNodeFlag;
				}

				if (node.ChildNodes.Count > 0)
				{
					RestoreChildeNodeState(node.ChildNodes);
				}
			}
		}

		//Обработка ивента изменения статуса ноды (открыта, закрыта), 
		//отрабатывает когда пользователь открывает или закрывает ноду.
		//Изменившаяся нода добавляется в словарь состояния дерева (если она там уже была - сначала удаляется.
		protected void MyTreeView_TreeNodeStateChanged(object sender, TreeNodeEventArgs e)
		{
			if (treeViewState.ContainsKey(e.Node.Text))
			{
				_ = treeViewState.Remove(e.Node.Text);
			}

			treeViewState.Add(e.Node.Text, e.Node.Expanded);
		}

		//Метод загрузки данных из БД в дерево
		private void GetTreeViewItems()
		{
			//Очистка нодов дерева перед перерисовкой
			MyTreeView.Nodes.Clear();

			//Подключение к базе данных
			string connString = ConfigurationManager.ConnectionStrings["TreeReorderProject"].ConnectionString;

			using (SqlConnection sqlConn = new SqlConnection(connString))
			{
				using (SqlDataAdapter dataAdapter = new SqlDataAdapter("spGetTreeViewItems", sqlConn))
				{
					dataAdapter.SelectCommand.CommandType = CommandType.StoredProcedure;

					using (DataSet dataSet = new DataSet())
					{
						//Заполнение набора данных dataSet полученными из БД данными
						_ = dataAdapter.Fill(dataSet);

						//Создание зависимости в dataSet, ParentID - зависит от ID
						_ = dataSet.Relations.Add("ChildRows", dataSet.Tables[0].Columns["ID"], dataSet.Tables[0].Columns["ParentID"]);

						//Каждая из строк в наборе данных dataSet проверяется на наличие ParentID. Если параметр отсутствует
						//в дереве создается "родительская" нода
						foreach (DataRow dataRow in dataSet.Tables[0].Rows)
						{
							if (string.IsNullOrEmpty(dataRow["ParentID"].ToString()))
							{
								//Создание ноды
								TreeNode parentTreeNode = new TreeNode
								{
									Text = dataRow["TreeViewText"].ToString(),
									Value = dataRow["ID"].ToString()
								};

								//проверка на наличие нодов-"потомков", их загрузка в "родительскую" ноду
								GetChildRows(dataRow, parentTreeNode);

								//Отправка ноды в дерево
								MyTreeView.Nodes.Add(parentTreeNode);
							}
						}
					}
				}
			}
		}

		//Метод проверки на наличие нодов-потомков и их загрузки в "родительскую ноду
		private void GetChildRows(DataRow dataRow, TreeNode treeNode)
		{
			DataRow[] childRows = dataRow.GetChildRows("ChildRows");

			foreach (DataRow childRow in childRows)
			{
				//Создание ноды
				TreeNode childTreeNode = new TreeNode
				{
					Text = childRow["TreeViewText"].ToString(),
					Value = childRow["ID"].ToString()
				};

				//Отправка ноды в "родительскую"
				treeNode.ChildNodes.Add(childTreeNode);

				//Проверка на наличие "потомков" у "потомка"
				if (childRow.GetChildRows("ChildRows").Length > 0)
				{
					GetChildRows(childRow, childTreeNode);
				}
			}
		}

		//Обработчик кнопк "Раскрыть все"
		protected void ExpandBtn_Click(object sendr, EventArgs e)
		{
			MyTreeView.ExpandAll();
		}

		//Обработчик кнопки "Свернуть все"
		protected void CollapseBtn_Click(object sendr, EventArgs e)
		{
			MyTreeView.CollapseAll();
		}

		[WebMethod]
		//Метод занесения данных, полученных в результате действий пользователя по переносу ноды, в БД
		public static string UpdateDB(string draggedNodeID, string droppedOnNodeID)
		{
			string connString = ConfigurationManager.ConnectionStrings["TreeReorderProject"].ConnectionString;

			using (SqlConnection sqlConn = new SqlConnection(connString))
			{
				sqlConn.Open();

				string sqlStatement = "UPDATE tblTreeViewNodes SET ParentID = " + droppedOnNodeID + " WHERE ID = " + draggedNodeID;

				using (SqlCommand cmd = new SqlCommand(sqlStatement, sqlConn))
				{
					_ = cmd.ExecuteNonQuery();
				}

				sqlConn.Close();

				//Возвращает строку запроса SQL для отладки
				return sqlStatement;
			}
		}
	}
}