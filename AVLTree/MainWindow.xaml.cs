using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;


public class Node
{
    public int Value;       // value of the node
    public Node Par;        // parent node
    public Node Left;       // left son node
    public Node Right;      // right son node
    public int Level;       // distance from the top -- used in visualisation
    public TextBlock TB;    // text block for visualization
    public int Height;      // distance from the bottom -- used for AVL condition control


    //constructor
    public Node(int v, int l, Node p = null)
    {
        Value = v;
        Level = l;
        Par = p;
        Left = null;
        Right = null;
        Height = 1;

        Random rnd = new Random();

        TB = new TextBlock()
        {
            FontSize = 20,
            Text = Convert.ToString(Value),
            MinWidth = 35,
            MinHeight = 30,
            Background = new SolidColorBrush(Color.FromArgb(70, Convert.ToByte(rnd.Next() % 255),
                                                                Convert.ToByte(rnd.Next() % 255),
                                                                Convert.ToByte(rnd.Next() % 255))),
            TextAlignment = TextAlignment.Center
        };
    }

    //update node`s height, updates all sons
    public void UpdateNodeHeight()
    {
        if (Left == null && Right == null)
        {
            Height = 1;
        }
        else if (Left == null && Right != null)
        {
            Right.UpdateNodeHeight();
            Height = Right.Height + 1;
        }
        else if (Left != null && Right == null)
        {
            Left.UpdateNodeHeight();
            Height = Left.Height + 1;
        }
        else
        {
            Left.UpdateNodeHeight();
            Right.UpdateNodeHeight();
            Height = Math.Max(Left.Height, Right.Height) + 1;
        }
    }

    //update parents height
    public void UpdateParentHeight()
    {
        if (Par != null)
        {            
            int h1 = 0;
            int h2 = 0;

            if (Par.Left != null) h1 = Par.Left.Height;
            if (Par.Right != null) h2 = Par.Right.Height;
            Par.Height = Math.Max(h1, h2) + 1;
            
            Par.UpdateParentHeight();
        }
    }

    //updates level of node
    public void UpdateLevel(int l)
    {
        Level = l;
        if (Left != null) Left.UpdateLevel(l + 1);
        if (Right != null) Right.UpdateLevel(l + 1);
    }

    //draws textblock on canvas, also for all sons
    public void Draw(ref Canvas _g, double LeftStart, double Width)
    {
        Canvas.SetLeft(TB, LeftStart + Width / 2);
        Canvas.SetTop(TB, 100 * Level + 30);
        _g.Children.Add(TB);


        if (Left != null)
        {
            Line L = new Line
            {
                Stroke = new SolidColorBrush(Colors.Black),
                StrokeThickness = 2,
                X1 = LeftStart + Width / 2,
                X2 = LeftStart + Width / 4 + 15,
                Y1 = 100 * Level + 60,
                Y2 = 100 * (Level + 1) + 30
            };
            _g.Children.Add(L);
            Left.Draw(ref _g, LeftStart, Width / 2);
        }
        if (Right != null)
        {
            Line L = new Line
            {
                Stroke = new SolidColorBrush(Colors.Black),
                StrokeThickness = 2,
                X1 = LeftStart + Width / 2 + 35,
                X2 = LeftStart + 3* Width / 4 + 15,
                Y1 = 100 * Level + 60,
                Y2 = 100 * (Level + 1) + 30
            };
            _g.Children.Add(L);
            Right.Draw(ref _g, LeftStart + Width / 2, Width / 2);
        }
    }

    //inserts new node according to its key
    public void Insert(int Key, ref Node Root)
    {
        if (Key < Value)
        {
            if (Left != null)
            {
                Left.Insert(Key, ref Root);
            }
            else
            {
                Left = new Node(Key, Level + 1, this);
                Left.UpdateParentHeight();                  
                AVLConditionParent(ref Root);

            }
        }
        else
        {
            if (Right != null)
            {
                Right.Insert(Key,ref Root);
            }
            else
            {
                Right = new Node(Key, Level + 1, this);
                Right.UpdateParentHeight();             
                AVLConditionParent(ref Root);
            }
        }
    }

    //finds first occurance node with value == key
    public Node Search(int Key)
    {
        if (Value == Key)   return this;


        if (Key < Value)
        {
            if (Left == null) return null;
            return Left.Search(Key);
        }

        if (Right == null) return null;
        return Right.Search(Key);
        
    }

    //finds node with min value in subtree
    public Node Minimum()
    {
        if (Left != null) return Left.Minimum();

        return this;
    }

    //finds node with max value in subtree
    public Node Maximum()
    {
        if (Right != null) return Right.Maximum();

        return this;
    }

    // finds node witch is right after node when all nodes in tree are in linear order
    public Node Succ()
    {
        if (Right != null) return Right.Minimum();

        if (Par == null) return null;

        Node P = Par;
        Node X = this;
        while (P != null && X == P.Right)
        {
            X = P;
            P = P.Par;
        }
        return P;

    }

    // finds node witch is right before node when all nodes in tree are in linear order
    public Node Pred()
    {
        if (Left != null) return Left.Maximum();

        if (Par == null) return null;

        Node P = Par;
        Node X = this;
        while(P != null && X == P.Left)
        {
            X = P;
            P = P.Par;
        }
        return P;
    }


    //replace node and its subtree with N and N`s subtree, new parent`s reference to son is N
    public void Replace(ref Node Root, Node N)
    {
        //replaces root with N, tree is now subtree of N
        if (this == Root)
        {
            Root = N;
            if (N != null)
            {
                N.Par = null;
                N.UpdateLevel(0);
            }
        }

        //replaces node and its subtree with null
        else if (N == null)
        {
            if (Par.Left == this)
            {
                Par.Left = null;
            }
            else
            {
                Par.Right = null;
            }
            return;
        }

        //replace one subtree of tree with another subtree rooted in N
        else
        {
            if (Par.Left == this)
            {
                Par.Left = N;
                N.UpdateLevel(Par.Level + 1);
            }
            else
            {
                Par.Right = N;
                N.UpdateLevel(Par.Level + 1);
            }
            N.Par = Par;
        }
    }

    //deletes node from tree, not with its subtree
    public void Delete(ref Node Root)
    {
        //if one of sons is null then we only replace our node with its son

        //replace with right son
        if (Left == null)
        {
            Replace(ref Root, Right);
            if (Right != null)
            {
                Right.UpdateNodeHeight();
                Right.UpdateParentHeight();   
            }
            else UpdateParentHeight();
            //control of AVL condition
            if (Par != null) AVLConditionParent(ref Root);
        }
        else
        {
            //replace with left son
            if (Right == null)
            {
                Replace(ref Root, Left);
                if (Left != null)
                {
                    Left.UpdateNodeHeight();
                    Left.UpdateParentHeight();          
                }
                else UpdateParentHeight();
                //control of AVL condition
                if (Par != null) AVLConditionParent(ref Root);
            }
            //node we want to delete has both sons, we`ll replace it by its successor 
            else
            {
                Node Successor = Succ();

                //if suucessor == right, means right son has no left son
                if (Successor == Right)
                {
                    Successor.Left = Left;
                    Left.Par = Successor;
                    Replace(ref Root, Successor);
                    Successor.UpdateNodeHeight();
                    Successor.UpdateParentHeight();          

                    //control of AVL condition to the root
                    Successor.AVLConditionNode(ref Root);
                    if (Successor.Par != null) Successor.AVLConditionParent(ref Root);
                }

                //successor is minimal node in right subtree, so it has no left son
                else
                {
                    Successor.Replace(ref Root, Successor.Right);      //replace successor by its right son

                    Successor.Right = Right;                           //change reference of successors right son to right son of deleted node
                    Right.Par = Successor;                             //change also reference of this right from this node to succ

                    Successor.Left = Left;                             //change also left son to be a son of succ
                    Left.Par = Successor;

                    Replace(ref Root, Successor);                      //replace this node with subtree with successor

                    Successor.UpdateNodeHeight();
                    Successor.UpdateParentHeight();             

                    //AVL condition control
                    Successor.Right.AVLConditionNode(ref Root);     
                    Successor.Right.AVLConditionParent(ref Root);
                }
            }
        }
    }

    //leftrotation of subtree
    public void Leftrotate(ref Node Root)
    {
        if (Right == null)
        {
            MessageBox.Show("There is not right son");
            return;
        }

        Node K = Right;
        Right = null;

        if (K.Left != null)
        {
            Right = K.Left;
            Right.Par = this;
        }

        K.Left = this;

        if (Par != null)
        {
            if (this == Par.Left)
            {
                Par.Left = K;
            }
            else
            {
                Par.Right = K;
            }
        }
        else
        {
            Root = K;
        }

        K.Par = Par;
        Par = K;

        K.UpdateLevel(Level);
        K.UpdateNodeHeight();
        K.UpdateParentHeight();
    }

    //rightrotation of subtree
    public void Rightrotate(ref Node Root)
    {
        if (Left == null)
        {
            MessageBox.Show("There is not left son");
            return;
        }

        Node K = Left;
        Left = null;

        if (K.Right != null)
        {
            Left = K.Right;
            Left.Par = this;
        }

        K.Right = this;

        if (Par != null)
        {
            if (this == Par.Right)
            {
                Par.Right = K;
            }
            else
            {
                Par.Left = K;
            }
        }
        else
        {
            Root = K;
        }

        K.Par = Par;
        Par = K;

        K.UpdateLevel(Level);
        K.UpdateNodeHeight();
        K.UpdateParentHeight();
    }

    public void AVLConditionNode(ref Node Root)
    {
        int Rheight;
        int Lheight;

        if (Right != null) Rheight = Right.Height;
        else Rheight = 0;

        if (Left != null) Lheight = Left.Height;
        else Lheight = 0;

        //condition is not violated
        if (Math.Abs(Rheight - Lheight) < 2) return;

        //condition is violated
        if (Lheight == Height - 1)
        {
            if (Left.Left != null && Left.Left.Height == Height - 2)
            {
                Rightrotate(ref Root);
            }
            else // Left.Right.Height == Height - 2
            {
                Left.Leftrotate(ref Root);
                Rightrotate(ref Root);
            }
        }
        else // Rheight == Height - 1
        {
            if(Right.Right !=null && Right.Right.Height == Height - 2)
            {
                Leftrotate(ref Root);
            }
            else // Right.Left.Height == Height - 2
            {
                Right.Rightrotate(ref Root);
                Leftrotate(ref Root);
            }
        }
    }

    public void AVLConditionParent(ref Node Root)
    {
        if(Par != null)
        {
            Par.AVLConditionNode(ref Root);
            if(Par != null) Par.AVLConditionParent(ref Root);
        }
    }

}






namespace AVLTree
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        bool Allowed = false;
        Node Root;



        public MainWindow()
        {
            InitializeComponent();
            Number.Text = "";
            Root = null;
        }

        // allow to write only numbers
        private void Number_PreviewKeyDown(object sender, KeyEventArgs e)
        {
            Allowed = true;

            //not number from keyboard
            if ((e.Key < Key.D0 || e.Key > Key.D9))
            {
                // not number from numpad
                if ((e.Key < Key.NumPad0 || e.Key > Key.NumPad9))
                {
                    //not backspace
                    if (!(e.Key == Key.Back))
                    {
                        Allowed = false;
                    }
                }
            }
        }

        private void Number_KeyDown(object sender, KeyEventArgs e)
        {
            if (!Allowed)
            {
                e.Handled = true;
            }
        }

        // insert node button
        private void Insert_Click(object sender, RoutedEventArgs e)
        {
            int Key = Convert.ToInt32(Number.Text.Trim());

            if (Root == null)
            {
                Root = new Node(Key, 0);
                g.Children.Clear();
                Root.Draw(ref g, 0.0, g.Width);
            }
            else
            {
                Root.Insert(Key, ref Root);
                g.Children.Clear();
                Root.Draw(ref g, 0.0, g.Width);
            }
        }

        // search node button
        private void Search_Click(object sender, RoutedEventArgs e)
        {
            int Key = Convert.ToInt32(Number.Text.Trim());
            

            if (Root != null)
            {
                g.Children.Clear();
                Root.Draw(ref g, 0.0, g.Width);

                Node N = Root.Search(Key);

                if (N != null)
                {
                    DrawEllipse(N);
                }
                else
                {
                    MessageBox.Show("This number is not in tree");
                }
            }
        }

        // minimum node button
        private void Minimum_node_Click(object sender, RoutedEventArgs e)
        {
            if (Root != null)
            {
                g.Children.Clear();
                Root.Draw(ref g, 0.0, g.Width);

                Node N = Root.Minimum();

                DrawEllipse(N);
            }
        }

        // maximum node button
        private void Maximum_node_Click(object sender, RoutedEventArgs e)
        {
            if (Root != null)
            {
                g.Children.Clear();
                Root.Draw(ref g, 0.0, g.Width);

                Node N = Root.Maximum();
                DrawEllipse(N);
            }
        }

        // succesor node button
        private void Successor_Click(object sender, RoutedEventArgs e)
        {
            int Key = Convert.ToInt32(Number.Text.Trim());

            if (Root != null)
            {
                g.Children.Clear();
                Root.Draw(ref g, 0.0, g.Width);

                //search first
                Node N = Root.Search(Key);
                Node Succ = null;

                //successor
                if (N != null)
                {
                    Succ = N.Succ();

                    if (Succ != null)
                    {
                        DrawEllipse(Succ);
                    }
                    else
                    {
                        MessageBox.Show("Number has no successor");
                    }
                }
                else
                {
                    MessageBox.Show("This number is not in tree");
                }
            }
        }

        // predecessor node button
        private void Predecessor_Click(object sender, RoutedEventArgs e)
        {
            int Key = Convert.ToInt32(Number.Text.Trim());

            if (Root != null)
            {
                g.Children.Clear();
                Root.Draw(ref g, 0.0, g.Width);

                //search first
                Node N = Root.Search(Key);
                Node Pred = null;

                //predecessor
                if (N != null)
                {
                    Pred = N.Pred();

                    if (Pred != null)
                    {
                        DrawEllipse(Pred);
                    }
                    else
                    {
                        MessageBox.Show("Number has no predecessor");
                    }
                }
                else
                {
                    MessageBox.Show("This number is not in tree");
                }
            }
        }

        // delete node button
        private void Delete_Click(object sender, RoutedEventArgs e)
        {
            int Key = Convert.ToInt32(Number.Text.Trim());

            if (Root != null)
            {
                //search first
                Node N = Root.Search(Key);


                if (N != null)
                {
                    N.Delete(ref Root);
                }
                else
                {
                    MessageBox.Show("This number is not in tree");
                }

                g.Children.Clear();
                if (Root != null) Root.Draw(ref g, 0.0, g.Width);
                
            }
        }

        // draw customized ellipse according to number to emphasize node
        private void DrawEllipse(Node n)
        {
            
            if (n.Value <= 999)
            {
                Ellipse R = new Ellipse
                {
                    Stroke = new SolidColorBrush(Colors.Red),
                    StrokeThickness = 2,
                    Width = 60,
                    Height = 60,
                };
                Canvas.SetLeft(R, Canvas.GetLeft(n.TB) - 13);
                Canvas.SetTop(R, Canvas.GetTop(n.TB) - 15);

                g.Children.Add(R);
            }
            else
            {
                Ellipse R = new Ellipse
                {
                    Stroke = new SolidColorBrush(Colors.Red),
                    StrokeThickness = 2,
                    Width = 2 * n.TB.ActualWidth,
                    Height = 60,
                };
                Canvas.SetLeft(R, Canvas.GetLeft(n.TB) - R.Width / 4);
                Canvas.SetTop(R, Canvas.GetTop(n.TB) - 15);

                g.Children.Add(R);
            }
        }




        /*
        private void Leftrotate_Click(object sender, RoutedEventArgs e)
        {
            int Key = Convert.ToInt32(Number.Text.Trim());

            if (Root != null)
            {
                //search first
                Node N = Root.Search(Key);

                if (N != null)
                {
                    N.Leftrotate(ref Root);
                }
                else
                {
                    MessageBox.Show("This number is not in tree");
                }
                g.Children.Clear();
                Root.Draw(g, 0.0, g.Width);

            }
        }

        private void Rightrotate_Click(object sender, RoutedEventArgs e)
        {
            int Key = Convert.ToInt32(Number.Text.Trim());

            if (Root != null)
            {
                //search first
                Node N = Root.Search(Key);

                if (N != null)
                {
                    N.Rightrotate(ref Root);
                }
                else
                {
                    MessageBox.Show("This number is not in tree");
                }
                g.Children.Clear();
                Root.Draw(g, 0.0, g.Width);

            }
        
        */
    }
}
