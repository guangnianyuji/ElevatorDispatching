using System.Drawing;
using System.Drawing.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.Runtime.Intrinsics.X86;

namespace ElevatorDispatching
{

    public partial class Form1 : Form
    {
        private FontFamily FML;

        private int elevatorNum;
        private int buildingHeight;
        private int floorPicLen;
        public class ElevatorControl
        {
            public Elevator elevator;
            public Button[] floorButton;

        };
        public ElevatorControl[] elevatorControl;
        private System.Windows.Forms.Timer algorithmTimer;

        private class ElevatorRequest
        {
            //起始楼层
            public int startFloor;
            //是否为上楼请求
            public bool up;
            //正在处理该请求的电梯
            public int elevatorIndex;
            //产生该请求的按钮（外部）
            public Button requestButton;

            //构造函数
            public ElevatorRequest(int startFloor, bool up, Button requestButton)
            {
                this.startFloor = startFloor;
                this.up = up;
                elevatorIndex = -1;
                this.requestButton = requestButton;
            }
        }

        private List<ElevatorRequest> requests;

        public Form1(int elevatorNum = 5, int buildingHeight = 20)
        {
            this.elevatorNum = elevatorNum;
            this.buildingHeight = buildingHeight;

            InitializeComponent();

            this.Load += new EventHandler(Form1_Load);
        }

        private void Form1_Load(object sender, EventArgs e)
        {
            //读取字体
            PrivateFontCollection pfc = new PrivateFontCollection();
            pfc.AddFontFile("img//digifaw.ttf");
            this.FML = pfc.Families[0];

            //调度算法执行频率
            algorithmTimer = new System.Windows.Forms.Timer();
            algorithmTimer.Interval = 1;
            algorithmTimer.Tick += timer1_Tick;
            algorithmTimer.Start();

            elevatorControl = new ElevatorControl[elevatorNum];
            //请求
            requests = new List<ElevatorRequest>();
            //为每部电梯加内部按钮
            this.floorPicLen = 50;
            Size floorButtonSize = new Size(60, floorPicLen - 5);

            for (int i = 0; i < elevatorNum; i++)
            {
                Point buildingPosition = new Point(120 + i * 200, 100);
                elevatorControl[i] = new ElevatorControl
                {
                    elevator = new Elevator(this, buildingHeight, buildingPosition, floorPicLen, FML),
                    floorButton = new Button[buildingHeight]
                };
                for (int j = 0; j < buildingHeight; ++j)
                {
                    elevatorControl[i].floorButton[j] = new Button
                    {
                        Size = floorButtonSize,
                        //楼层数
                        Text = (buildingHeight - j).ToString(),
                        Name = (i).ToString() + " " + (buildingHeight - j).ToString(),
                        Parent = this,
                        Location =
                        new Point(buildingPosition.X + floorPicLen + 10, buildingPosition.Y + floorPicLen * j),
                        Font = new Font(FML, 8)

                    };

                    //内部按钮事件
                    elevatorControl[i].floorButton[j].Click += insideRequest;

                }
            };

            //最右侧加外部按钮
            Size moveButtonSize = new Size(40, 40);
            for (int j = 0; j < buildingHeight; ++j)
            {
                if (j != 0)
                {
                    Button upButton = new Button
                    {
                        Size = moveButtonSize,
                        Text = "",
                        Name = "+" + (buildingHeight - j).ToString(),
                        Location = new Point(1100, 100 + floorPicLen * j),
                        BackgroundImage = Image.FromFile("img//向上.png"),
                        FlatStyle = FlatStyle.Flat,
                        BackgroundImageLayout = ImageLayout.Stretch,
                        Parent = this,
                    };
                    //外部按钮事件
                    upButton.Click += outsideRequest;
                }
                if (j != buildingHeight - 1)
                {
                    Button downButton = new Button
                    {
                        Size = moveButtonSize,
                        Text = "",
                        Name = "-" + (buildingHeight - j).ToString(),
                        Location = new Point(1150, 100 + floorPicLen * j),
                        BackgroundImage = Image.FromFile("img//向下.png"),
                        FlatStyle = FlatStyle.Flat,
                        BackgroundImageLayout = ImageLayout.Stretch,
                        Parent = this,
                    };
                    //外部按钮事件
                    downButton.Click += outsideRequest;
                }

            }



            Label messageLabel = new Label
            {
                Font = new Font(FML, 13),
                Text = "2152085 孙亦菲 操作系统项目01——电梯调度",
                Parent = this,
                Location = new Point(100, 0),
                AutoSize = true
            };
        }

        //内部按钮
        private void insideRequest(object sender, EventArgs e)
        {

            Button insideButton = (Button)sender;

            //获取编号和楼层信息
            string[] s = insideButton.Name.Split();
            int index = int.Parse(s[0]);//转换为整数
            int floor = int.Parse(s[1]);


            this.elevatorControl[index].elevator.stopFloor[floor] = true;//电梯要在这停
            this.elevatorControl[index].elevator.insideButton.Add(insideButton);

            insideButton.Enabled = false;
        }

        private void outsideRequest(object sender, EventArgs e)
        {
            Button outsideButton = (Button)sender;
            outsideButton.Enabled = false;

            bool toUp = outsideButton.Name[0] == '+';
            int startFloor = int.Parse(outsideButton.Name.Substring(1));
            if (toUp)//按钮为点亮状态
            {
                outsideButton.BackgroundImage = Image.FromFile("img//向上-click.png");
            }
            else
            {
                outsideButton.BackgroundImage = Image.FromFile("img//向下-click.png");
            }
            //调度算法
            addRequest(startFloor, toUp, outsideButton);
        }

        //增加命令
        void addRequest(int startFloor, bool toUp, Button requestButton)
        {
            //将其加入到request结构体中
            ElevatorRequest request = new ElevatorRequest(startFloor, toUp, requestButton);
            requests.Add(request);
        }



        //扫描算法
        void scanAlgorithm()
        {
            for (int i = 0; i < requests.Count; i++)
            {
                if (requests[i].elevatorIndex != -1)
                {
                    continue;
                }
                //寻找到距离最近运行方向相同或目前暂停的电梯
                int minIndex = -1;
                int minDist = buildingHeight + 1;
                System.Diagnostics.Trace.WriteLine(requests[i].up);
                for (int j = 0; j < elevatorNum; ++j)
                {   //该电梯警报状态，不考虑
                    if (elevatorControl[j].elevator.elevatorMovingState == Elevator.ELEVATORMOVINGSTATE.ALARM)
                    {
                        continue;
                    }
                    //等待状态的电梯，可以直接进行考虑
                    int Dist = Math.Abs(elevatorControl[j].elevator.floorCurrent - requests[i].startFloor);
                    if (elevatorControl[j].elevator.elevatorMovingState == Elevator.ELEVATORMOVINGSTATE.WAIT)
                    {
                        if (Dist < minDist)
                        {
                            minDist = Dist;
                            minIndex = j;
                        }
                    }
                    //如果是向上需求,找也向上的电梯且当前楼层等于小于
                    if (requests[i].up && elevatorControl[j].elevator.elevatorMovingState == Elevator.ELEVATORMOVINGSTATE.UP
                       && elevatorControl[j].elevator.floorCurrent <= requests[i].startFloor)
                    {
                        if (Dist < minDist)
                        {
                            minDist = Dist;
                            minIndex = j;
                        }
                    }
                    //如果是向下需求，找也向下的电梯且当前楼层大于等于
                    if (!requests[i].up && elevatorControl[j].elevator.elevatorMovingState == Elevator.ELEVATORMOVINGSTATE.DOWN
                      && elevatorControl[j].elevator.floorCurrent >= requests[i].startFloor)
                    {
                        if (Dist < minDist)
                        {
                            minDist = Dist;
                            minIndex = j;
                        }
                    }

                }
                //找到了可调度电梯
                if (minIndex != -1)
                {
                    requests[i].elevatorIndex = minIndex;
                    elevatorControl[minIndex].elevator.stopFloor[requests[i].startFloor] = true;//该电梯要在这停
                    elevatorControl[minIndex].elevator.outsideButton.Add(requests[i].requestButton); 

                    requests.RemoveAt(i);
                    //为防止一部电梯响应多个，找到就退出
                    return;
                }
            }

            //确认已完成的请求
            for (int i = 0; i < requests.Count(); ++i)
            {
                if (requests[i].requestButton.Enabled)
                {
                    requests.RemoveAt(i);
                    return;
                }
            }
        }
        private void timer1_Tick(object sender, EventArgs e)
        {
            scanAlgorithm();
        }

 
    }


    public class Elevator
    {
        public int floorCurrent;//当前楼层
        private PictureBox[] floorPic;//楼层图像
        private int floorMax;//可以在的最大楼层

        private Point buildingLocation;//建筑所在位置

        //电梯图像大小
        private int floorPicLen;


        //需要停靠的楼层编号
        public bool[] stopFloor;

        //绑定外部按钮
        public List<Button> outsideButton;
        //绑定内部按钮
        public List<Button> insideButton;

        Form parent;

        //电梯图
        PictureBox eleImage;

        //电梯整体运动状态
        public enum ELEVATORMOVINGSTATE
        {
            UP, DOWN, WAIT,ALARM
        }
        public ELEVATORMOVINGSTATE elevatorMovingState;
        public ELEVATORMOVINGSTATE lastelevatorMovingState;

        //显示楼层
        public Label floorLabel;


        private System.Windows.Forms.Timer movingTimer;//移动计时器
        private System.Windows.Forms.Timer globalTimer;//全局计时器
        private int doorWaiting;//模拟电梯开关所需的时间
        private System.Windows.Forms.Timer doorTimer;//全局计时器

        private int movingDis;//每次移动的距离

        Button alarmButton;//报警按钮
        Button passButton;//通行按钮

        Button openButton;//开门按钮
        Button closeButton;//关门按钮

        public Elevator(Form parent, int floorMax, Point buildingLocation,
           int floorPicLen, FontFamily FML)
        {
            this.floorCurrent = 1;
            this.floorMax = floorMax;

            //建筑初始化
            this.buildingLocation = buildingLocation;
            //根据楼层数生成楼层
            this.floorPic = new PictureBox[floorMax];

            //楼层图片的长度
            this.floorPicLen = floorPicLen;
            //楼层图片的大小
            Size floorPicSize = new Size(floorPicLen, floorPicLen);

            //需要停靠的楼层编号
            this.stopFloor = new bool[floorMax + 1];
            for (int i = 1; i <= floorMax; ++i)
            {
                this.stopFloor[i] = false;
            }
            this.outsideButton = new List<Button>();
            this.insideButton = new List<Button>();
            //父控件
            this.parent = parent;

            //楼层图
            for (int i = 0; i < floorMax; i++)
            {
                this.floorPic[i] = new PictureBox
                {
                    Location = new Point(buildingLocation.X,
                                  buildingLocation.Y + i * floorPicLen),
                    Size = floorPicSize,
                    Image = Image.FromFile("img//black.png"),
                    Parent = parent,
                    SizeMode = PictureBoxSizeMode.StretchImage
                };
            }
            //电梯图
            this.eleImage = new PictureBox
            {
                Image = Image.FromFile("img//eleclose.png"),
                Parent = parent,
                Location = new Point(buildingLocation.X,
                                  buildingLocation.Y + (floorMax - floorCurrent) * floorPicLen),
                Size = floorPicSize,
                SizeMode = PictureBoxSizeMode.StretchImage//图片自动填充size
            };
            this.eleImage.BringToFront();

            //电梯状态初始化
            this.elevatorMovingState = ELEVATORMOVINGSTATE.WAIT;
            //电梯楼层显示
            floorLabel = new Label
            {
                Text = "01",
                Parent = parent,
                Location = new Point(buildingLocation.X + 20, buildingLocation.Y - 50),
                Size = new Size(floorPicLen * 2, floorPicLen),
                BackColor = Color.White
            };

            floorLabel.Font = new Font(FML, 15);
            floorLabel.BringToFront();

            alarmButton = new Button
            {
                Parent = parent,
                BackgroundImage = Image.FromFile("img//警报2.png"),
                FlatStyle = FlatStyle.Flat,
                BackgroundImageLayout = ImageLayout.Stretch,
                Location = new Point(buildingLocation.X, buildingLocation.Y + (floorMax + 2) * floorPicLen + 10),
                Size = new Size(floorPicLen, floorPicLen)
            };
            alarmButton.Click += alarmRequest;

            passButton = new Button
            {
                Parent = parent,
                BackgroundImage = Image.FromFile("img//人.png"),
                FlatStyle = FlatStyle.Flat,
                BackgroundImageLayout = ImageLayout.Stretch,
                Location = new Point(buildingLocation.X + floorPicLen + 10, buildingLocation.Y + (floorMax + 2) * floorPicLen + 10),
                Size = new Size(floorPicLen, floorPicLen)
            };
            passButton.Enabled = false;
            passButton.Click += passRequest;

            openButton = new Button
            {
                Parent = parent,
                BackgroundImage = Image.FromFile("img//open.png"),


                FlatStyle = FlatStyle.Flat,
                BackgroundImageLayout = ImageLayout.Stretch,
                Location = new Point(buildingLocation.X, buildingLocation.Y + (floorMax + 1) * floorPicLen),
                Size = new Size(floorPicLen, floorPicLen)
            };
            openButton.Click += openRequest;

            closeButton = new Button
            {
                Parent = parent,
                BackgroundImage = Image.FromFile("img//close.png"),


                FlatStyle = FlatStyle.Flat,
                BackgroundImageLayout = ImageLayout.Stretch,
                Location = new Point(buildingLocation.X + floorPicLen + 10, buildingLocation.Y + (floorMax + 1) * floorPicLen),
                Size = new Size(floorPicLen, floorPicLen)
            };
            closeButton.Click += closeRequest;

            //移动计时器，为电梯移动效果
            this.movingTimer = new System.Windows.Forms.Timer
            {
                Interval = 10
            };
            this.movingTimer.Tick += movingTimerTick;
            this.movingDis = 0;

            //全局计时器，为处理内部外部按钮
            this.globalTimer = new System.Windows.Forms.Timer
            {
                Interval = 1
            };
            this.globalTimer.Tick += this.globalTimerTick;
            this.globalTimer.Start();

            doorTimer = new System.Windows.Forms.Timer
            {
                Interval = 30
            };
            this.doorTimer.Tick += this.doorTimerTick;
            doorTimer.Start();
            doorWaiting = 0;
        }

        //按钮任务完成处理
        private void buttonEndDeal()
        {
            //当门关闭后，处理外部按钮
            for (int i = 0; i < outsideButton.Count(); ++i)
            {
                if (int.Parse(this.outsideButton[i].Name.Substring(1)) == floorCurrent)
                {
                    outsideButton[i].Enabled = true;
                    if (outsideButton[i].Name[0] == '+')
                    {
                        outsideButton[i].BackgroundImage = Image.FromFile("img//向上.png");
                    }
                    else
                    {
                        outsideButton[i].BackgroundImage = Image.FromFile("img//向下.png");
                    }
                    outsideButton.RemoveAt(i);
                    break;
                }
            }

            //当门关闭后，处理内部按钮
            for (int i = 0; i < insideButton.Count(); ++i)
            {
                if (int.Parse(this.insideButton[i].Name.Substring(1)) == this.floorCurrent)
                {
                    insideButton[i].Enabled = true;
                    insideButton.RemoveAt(i);
                    break;
                }
            }
        }

        private void alarmRequest(object sender, EventArgs e)
        {
            alarmButton.BackgroundImage = Image.FromFile("img//警报1.png");
            globalTimer.Stop();
            movingTimer.Stop();
            alarmButton.Enabled = false;
            passButton.Enabled = true;
            passButton.BackgroundImage = Image.FromFile("img//等待.png");

            openButton.Enabled = false;
            closeButton.Enabled = false;

            lastelevatorMovingState = elevatorMovingState;
            elevatorMovingState = ELEVATORMOVINGSTATE.ALARM;
        }

        private void passRequest(object sender, EventArgs e)
        {
            alarmButton.BackgroundImage = Image.FromFile("img//警报2.png");
            globalTimer.Start();
            movingTimer.Start();
            alarmButton.Enabled = true;
            passButton.Enabled = false;
            passButton.BackgroundImage = Image.FromFile("img//人.png");

            openButton.Enabled = true;
            closeButton.Enabled = true;
            elevatorMovingState = lastelevatorMovingState;
        }
        private void openRequest(object sender, EventArgs e)
        {
            System.Diagnostics.Trace.WriteLine("re");
            if (doorWaiting > 0)//到达了一个楼层且正在开关门
            {
                if (doorWaiting < 26)//若关门
                {
                    doorWaiting = 61 - doorWaiting;//改变状态
                }
                return;
            }
            if (elevatorMovingState == ELEVATORMOVINGSTATE.WAIT)//正在等待
            {
                doorWaiting = 60;

            }
        }

        private void closeRequest(object sender, EventArgs e)
        {
            if (doorWaiting > 0)//到达了一个楼层且正在开关门
            {
                if (doorWaiting > 26)//若开门
                {
                    doorWaiting = 61 - doorWaiting;//改变状态
                }
                return;
            }
        }
        private void globalTimerTick(object sender, EventArgs e)
        {
            if (doorWaiting>0)//到一个楼层且需停靠时，处理按钮,该中间值无意义
            {
                buttonEndDeal();
            }

            if (this.elevatorMovingState == ELEVATORMOVINGSTATE.WAIT)
            {

                for (int i = 1; i <= floorMax; ++i)
                {
                    if (this.stopFloor[i])
                    {
                        if (i < floorCurrent)
                        {
                            this.elevatorMovingState = ELEVATORMOVINGSTATE.DOWN;

                        }
                        else if (i > floorCurrent)
                        {
                            this.elevatorMovingState = ELEVATORMOVINGSTATE.UP;
                        }
                        else
                        {
                            stay();//仍是等待状态
                        }
                        return;
                    }
                }
            }
            if (this.elevatorMovingState == ELEVATORMOVINGSTATE.UP)
            {
                //检查所有上方楼层是否需要去
                bool need = false;
                for (int i = this.floorCurrent + 1; i <= this.floorMax; ++i)
                {
                    if (this.stopFloor[i])
                    {
                        need = true;
                        break;
                    }
                }
                if (!need)
                {
                    this.elevatorMovingState = ELEVATORMOVINGSTATE.WAIT;
                    this.movingTimer.Stop();
                    return;
                }
                //调用向上移动函数
                //System.Diagnostics.Trace.WriteLine("UP");

                this.goUp();
            }
            if (this.elevatorMovingState == ELEVATORMOVINGSTATE.DOWN)
            {
                //检查所有下方楼层是否需要去
                bool need = false;
                for (int i = this.floorCurrent - 1; i > 0; --i)
                {
                    if (this.stopFloor[i])
                    {
                        need = true;

                        break;
                    }
                }
                if (!need)
                {
                    this.elevatorMovingState = ELEVATORMOVINGSTATE.WAIT;
                    this.movingTimer.Stop();
                    return;
                }
                //调用向下移动函数

                this.goDown();
            }
        }

        public bool goUp()
        {
            //查看当前状态是否允许上移
            if (this.floorCurrent == this.floorMax)
            {
                return false;
            }
            this.elevatorMovingState = ELEVATORMOVINGSTATE.UP;
            this.movingTimer.Start();


            return true;
        }
        public bool goDown()
        {
            //查看当前状态是否允许下移
            if (this.floorCurrent == 1)
            {
                return false;
            }
            this.elevatorMovingState = ELEVATORMOVINGSTATE.DOWN;
            this.movingTimer.Start();


            return true;
        }
        public void stay()
        {
            this.movingTimer.Start();
        }

        private void doorTimerTick(object sender, EventArgs e)
        {
            System.Diagnostics.Trace.WriteLine("doorWaiting");
            System.Diagnostics.Trace.WriteLine(doorWaiting);
            if (doorWaiting <= 0)
            {
                return;
            }

            doorWaiting--;
            System.Diagnostics.Trace.WriteLine("doorWaiting");
            System.Diagnostics.Trace.WriteLine(doorWaiting);
            if (doorWaiting == 55)
            {
                eleImage.Image = Image.FromFile("img//close1.png");
            }
            if (doorWaiting == 45)
            {
                eleImage.Image = Image.FromFile("img//close2.png");
            }
            if (doorWaiting == 36)
            {
                eleImage.Image = Image.FromFile("img//close3.png");//最大开门状态
            }
            if (doorWaiting == 15)
            {
                eleImage.Image = Image.FromFile("img//close2.png");
            }
            if (doorWaiting == 5)
            {
                eleImage.Image = Image.FromFile("img//close1.png");
            }
            if (doorWaiting == 0)
            {
                eleImage.Image = Image.FromFile("img//eleclose.png");
            }
        }
        private void movingTimerTick(object sender, EventArgs e)
        {
            if (doorWaiting > 0)
            {
                return;
            }

            //如果当前状态为向上移动,则Y坐标+1
            if (this.elevatorMovingState == ELEVATORMOVINGSTATE.UP)
            {
                //System.Diagnostics.Trace.WriteLine("upup");
                int X = eleImage.Location.X;
                int newY = eleImage.Location.Y - 2;
                eleImage.Location = new Point(X, newY);
                movingDis = movingDis + 2;
            }
            else if (this.elevatorMovingState == ELEVATORMOVINGSTATE.DOWN)
            {
                int X = eleImage.Location.X;
                int newY = eleImage.Location.Y + 2;
                eleImage.Location = new Point(X, newY);
                movingDis = movingDis + 2;
            }
            
            //System.Diagnostics.Trace.WriteLine(movingDis);
            if (movingDis == floorPicLen)
            {

                this.movingDis = 0;
                //一个楼层已经移动完毕
                if (this.elevatorMovingState == ELEVATORMOVINGSTATE.UP)
                {
                    this.floorCurrent++;
                }
                else if (this.elevatorMovingState == ELEVATORMOVINGSTATE.DOWN)
                {
                    this.floorCurrent--;
                }

                //楼层标识
                if (this.floorCurrent < 10)
                {
                    this.floorLabel.Text = "0" + (this.floorCurrent).ToString();
                }
                else
                {
                    this.floorLabel.Text = (this.floorCurrent).ToString();
                }

                //暂停条件
                //1.本层楼需要暂停(当前运行方向和外部指令方向相同)
                //2.所有上面的命令执行完毕


  
            }

            if (this.stopFloor[this.floorCurrent])
            {
                this.stopFloor[this.floorCurrent] = false;

                doorWaiting = 60;

            }
        }
    }
}