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
            //��ʼ¥��
            public int startFloor;
            //�Ƿ�Ϊ��¥����
            public bool up;
            //���ڴ��������ĵ���
            public int elevatorIndex;
            //����������İ�ť���ⲿ��
            public Button requestButton;

            //���캯��
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
            //��ȡ����
            PrivateFontCollection pfc = new PrivateFontCollection();
            pfc.AddFontFile("img//digifaw.ttf");
            this.FML = pfc.Families[0];

            //�����㷨ִ��Ƶ��
            algorithmTimer = new System.Windows.Forms.Timer();
            algorithmTimer.Interval = 1;
            algorithmTimer.Tick += timer1_Tick;
            algorithmTimer.Start();

            elevatorControl = new ElevatorControl[elevatorNum];
            //����
            requests = new List<ElevatorRequest>();
            //Ϊÿ�����ݼ��ڲ���ť
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
                        //¥����
                        Text = (buildingHeight - j).ToString(),
                        Name = (i).ToString() + " " + (buildingHeight - j).ToString(),
                        Parent = this,
                        Location =
                        new Point(buildingPosition.X + floorPicLen + 10, buildingPosition.Y + floorPicLen * j),
                        Font = new Font(FML, 8)

                    };

                    //�ڲ���ť�¼�
                    elevatorControl[i].floorButton[j].Click += insideRequest;

                }
            };

            //���Ҳ���ⲿ��ť
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
                        BackgroundImage = Image.FromFile("img//����.png"),
                        FlatStyle = FlatStyle.Flat,
                        BackgroundImageLayout = ImageLayout.Stretch,
                        Parent = this,
                    };
                    //�ⲿ��ť�¼�
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
                        BackgroundImage = Image.FromFile("img//����.png"),
                        FlatStyle = FlatStyle.Flat,
                        BackgroundImageLayout = ImageLayout.Stretch,
                        Parent = this,
                    };
                    //�ⲿ��ť�¼�
                    downButton.Click += outsideRequest;
                }

            }



            Label messageLabel = new Label
            {
                Font = new Font(FML, 13),
                Text = "2152085 ����� ����ϵͳ��Ŀ01�������ݵ���",
                Parent = this,
                Location = new Point(100, 0),
                AutoSize = true
            };
        }

        //�ڲ���ť
        private void insideRequest(object sender, EventArgs e)
        {

            Button insideButton = (Button)sender;

            //��ȡ��ź�¥����Ϣ
            string[] s = insideButton.Name.Split();
            int index = int.Parse(s[0]);//ת��Ϊ����
            int floor = int.Parse(s[1]);


            this.elevatorControl[index].elevator.stopFloor[floor] = true;//����Ҫ����ͣ
            this.elevatorControl[index].elevator.insideButton.Add(insideButton);

            insideButton.Enabled = false;
        }

        private void outsideRequest(object sender, EventArgs e)
        {
            Button outsideButton = (Button)sender;
            outsideButton.Enabled = false;

            bool toUp = outsideButton.Name[0] == '+';
            int startFloor = int.Parse(outsideButton.Name.Substring(1));
            if (toUp)//��ťΪ����״̬
            {
                outsideButton.BackgroundImage = Image.FromFile("img//����-click.png");
            }
            else
            {
                outsideButton.BackgroundImage = Image.FromFile("img//����-click.png");
            }
            //�����㷨
            addRequest(startFloor, toUp, outsideButton);
        }

        //��������
        void addRequest(int startFloor, bool toUp, Button requestButton)
        {
            //������뵽request�ṹ����
            ElevatorRequest request = new ElevatorRequest(startFloor, toUp, requestButton);
            requests.Add(request);
        }



        //ɨ���㷨
        void scanAlgorithm()
        {
            for (int i = 0; i < requests.Count; i++)
            {
                if (requests[i].elevatorIndex != -1)
                {
                    continue;
                }
                //Ѱ�ҵ�����������з�����ͬ��Ŀǰ��ͣ�ĵ���
                int minIndex = -1;
                int minDist = buildingHeight + 1;
                System.Diagnostics.Trace.WriteLine(requests[i].up);
                for (int j = 0; j < elevatorNum; ++j)
                {   //�õ��ݾ���״̬��������
                    if (elevatorControl[j].elevator.elevatorMovingState == Elevator.ELEVATORMOVINGSTATE.ALARM)
                    {
                        continue;
                    }
                    //�ȴ�״̬�ĵ��ݣ�����ֱ�ӽ��п���
                    int Dist = Math.Abs(elevatorControl[j].elevator.floorCurrent - requests[i].startFloor);
                    if (elevatorControl[j].elevator.elevatorMovingState == Elevator.ELEVATORMOVINGSTATE.WAIT)
                    {
                        if (Dist < minDist)
                        {
                            minDist = Dist;
                            minIndex = j;
                        }
                    }
                    //�������������,��Ҳ���ϵĵ����ҵ�ǰ¥�����С��
                    if (requests[i].up && elevatorControl[j].elevator.elevatorMovingState == Elevator.ELEVATORMOVINGSTATE.UP
                       && elevatorControl[j].elevator.floorCurrent <= requests[i].startFloor)
                    {
                        if (Dist < minDist)
                        {
                            minDist = Dist;
                            minIndex = j;
                        }
                    }
                    //���������������Ҳ���µĵ����ҵ�ǰ¥����ڵ���
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
                //�ҵ��˿ɵ��ȵ���
                if (minIndex != -1)
                {
                    requests[i].elevatorIndex = minIndex;
                    elevatorControl[minIndex].elevator.stopFloor[requests[i].startFloor] = true;//�õ���Ҫ����ͣ
                    elevatorControl[minIndex].elevator.outsideButton.Add(requests[i].requestButton); 

                    requests.RemoveAt(i);
                    //Ϊ��ֹһ��������Ӧ������ҵ����˳�
                    return;
                }
            }

            //ȷ������ɵ�����
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
        public int floorCurrent;//��ǰ¥��
        private PictureBox[] floorPic;//¥��ͼ��
        private int floorMax;//�����ڵ����¥��

        private Point buildingLocation;//��������λ��

        //����ͼ���С
        private int floorPicLen;


        //��Ҫͣ����¥����
        public bool[] stopFloor;

        //���ⲿ��ť
        public List<Button> outsideButton;
        //���ڲ���ť
        public List<Button> insideButton;

        Form parent;

        //����ͼ
        PictureBox eleImage;

        //���������˶�״̬
        public enum ELEVATORMOVINGSTATE
        {
            UP, DOWN, WAIT,ALARM
        }
        public ELEVATORMOVINGSTATE elevatorMovingState;
        public ELEVATORMOVINGSTATE lastelevatorMovingState;

        //��ʾ¥��
        public Label floorLabel;


        private System.Windows.Forms.Timer movingTimer;//�ƶ���ʱ��
        private System.Windows.Forms.Timer globalTimer;//ȫ�ּ�ʱ��
        private int doorWaiting;//ģ����ݿ��������ʱ��
        private System.Windows.Forms.Timer doorTimer;//ȫ�ּ�ʱ��

        private int movingDis;//ÿ���ƶ��ľ���

        Button alarmButton;//������ť
        Button passButton;//ͨ�а�ť

        Button openButton;//���Ű�ť
        Button closeButton;//���Ű�ť

        public Elevator(Form parent, int floorMax, Point buildingLocation,
           int floorPicLen, FontFamily FML)
        {
            this.floorCurrent = 1;
            this.floorMax = floorMax;

            //������ʼ��
            this.buildingLocation = buildingLocation;
            //����¥��������¥��
            this.floorPic = new PictureBox[floorMax];

            //¥��ͼƬ�ĳ���
            this.floorPicLen = floorPicLen;
            //¥��ͼƬ�Ĵ�С
            Size floorPicSize = new Size(floorPicLen, floorPicLen);

            //��Ҫͣ����¥����
            this.stopFloor = new bool[floorMax + 1];
            for (int i = 1; i <= floorMax; ++i)
            {
                this.stopFloor[i] = false;
            }
            this.outsideButton = new List<Button>();
            this.insideButton = new List<Button>();
            //���ؼ�
            this.parent = parent;

            //¥��ͼ
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
            //����ͼ
            this.eleImage = new PictureBox
            {
                Image = Image.FromFile("img//eleclose.png"),
                Parent = parent,
                Location = new Point(buildingLocation.X,
                                  buildingLocation.Y + (floorMax - floorCurrent) * floorPicLen),
                Size = floorPicSize,
                SizeMode = PictureBoxSizeMode.StretchImage//ͼƬ�Զ����size
            };
            this.eleImage.BringToFront();

            //����״̬��ʼ��
            this.elevatorMovingState = ELEVATORMOVINGSTATE.WAIT;
            //����¥����ʾ
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
                BackgroundImage = Image.FromFile("img//����2.png"),
                FlatStyle = FlatStyle.Flat,
                BackgroundImageLayout = ImageLayout.Stretch,
                Location = new Point(buildingLocation.X, buildingLocation.Y + (floorMax + 2) * floorPicLen + 10),
                Size = new Size(floorPicLen, floorPicLen)
            };
            alarmButton.Click += alarmRequest;

            passButton = new Button
            {
                Parent = parent,
                BackgroundImage = Image.FromFile("img//��.png"),
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

            //�ƶ���ʱ����Ϊ�����ƶ�Ч��
            this.movingTimer = new System.Windows.Forms.Timer
            {
                Interval = 10
            };
            this.movingTimer.Tick += movingTimerTick;
            this.movingDis = 0;

            //ȫ�ּ�ʱ����Ϊ�����ڲ��ⲿ��ť
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

        //��ť������ɴ���
        private void buttonEndDeal()
        {
            //���Źرպ󣬴����ⲿ��ť
            for (int i = 0; i < outsideButton.Count(); ++i)
            {
                if (int.Parse(this.outsideButton[i].Name.Substring(1)) == floorCurrent)
                {
                    outsideButton[i].Enabled = true;
                    if (outsideButton[i].Name[0] == '+')
                    {
                        outsideButton[i].BackgroundImage = Image.FromFile("img//����.png");
                    }
                    else
                    {
                        outsideButton[i].BackgroundImage = Image.FromFile("img//����.png");
                    }
                    outsideButton.RemoveAt(i);
                    break;
                }
            }

            //���Źرպ󣬴����ڲ���ť
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
            alarmButton.BackgroundImage = Image.FromFile("img//����1.png");
            globalTimer.Stop();
            movingTimer.Stop();
            alarmButton.Enabled = false;
            passButton.Enabled = true;
            passButton.BackgroundImage = Image.FromFile("img//�ȴ�.png");

            openButton.Enabled = false;
            closeButton.Enabled = false;

            lastelevatorMovingState = elevatorMovingState;
            elevatorMovingState = ELEVATORMOVINGSTATE.ALARM;
        }

        private void passRequest(object sender, EventArgs e)
        {
            alarmButton.BackgroundImage = Image.FromFile("img//����2.png");
            globalTimer.Start();
            movingTimer.Start();
            alarmButton.Enabled = true;
            passButton.Enabled = false;
            passButton.BackgroundImage = Image.FromFile("img//��.png");

            openButton.Enabled = true;
            closeButton.Enabled = true;
            elevatorMovingState = lastelevatorMovingState;
        }
        private void openRequest(object sender, EventArgs e)
        {
            System.Diagnostics.Trace.WriteLine("re");
            if (doorWaiting > 0)//������һ��¥�������ڿ�����
            {
                if (doorWaiting < 26)//������
                {
                    doorWaiting = 61 - doorWaiting;//�ı�״̬
                }
                return;
            }
            if (elevatorMovingState == ELEVATORMOVINGSTATE.WAIT)//���ڵȴ�
            {
                doorWaiting = 60;

            }
        }

        private void closeRequest(object sender, EventArgs e)
        {
            if (doorWaiting > 0)//������һ��¥�������ڿ�����
            {
                if (doorWaiting > 26)//������
                {
                    doorWaiting = 61 - doorWaiting;//�ı�״̬
                }
                return;
            }
        }
        private void globalTimerTick(object sender, EventArgs e)
        {
            if (doorWaiting>0)//��һ��¥������ͣ��ʱ������ť,���м�ֵ������
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
                            stay();//���ǵȴ�״̬
                        }
                        return;
                    }
                }
            }
            if (this.elevatorMovingState == ELEVATORMOVINGSTATE.UP)
            {
                //��������Ϸ�¥���Ƿ���Ҫȥ
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
                //���������ƶ�����
                //System.Diagnostics.Trace.WriteLine("UP");

                this.goUp();
            }
            if (this.elevatorMovingState == ELEVATORMOVINGSTATE.DOWN)
            {
                //��������·�¥���Ƿ���Ҫȥ
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
                //���������ƶ�����

                this.goDown();
            }
        }

        public bool goUp()
        {
            //�鿴��ǰ״̬�Ƿ���������
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
            //�鿴��ǰ״̬�Ƿ���������
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
                eleImage.Image = Image.FromFile("img//close3.png");//�����״̬
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

            //�����ǰ״̬Ϊ�����ƶ�,��Y����+1
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
                //һ��¥���Ѿ��ƶ����
                if (this.elevatorMovingState == ELEVATORMOVINGSTATE.UP)
                {
                    this.floorCurrent++;
                }
                else if (this.elevatorMovingState == ELEVATORMOVINGSTATE.DOWN)
                {
                    this.floorCurrent--;
                }

                //¥���ʶ
                if (this.floorCurrent < 10)
                {
                    this.floorLabel.Text = "0" + (this.floorCurrent).ToString();
                }
                else
                {
                    this.floorLabel.Text = (this.floorCurrent).ToString();
                }

                //��ͣ����
                //1.����¥��Ҫ��ͣ(��ǰ���з�����ⲿָ�����ͬ)
                //2.�������������ִ�����


  
            }

            if (this.stopFloor[this.floorCurrent])
            {
                this.stopFloor[this.floorCurrent] = false;

                doorWaiting = 60;

            }
        }
    }
}