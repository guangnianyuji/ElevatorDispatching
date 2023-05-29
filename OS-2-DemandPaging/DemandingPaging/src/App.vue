 <template>
	<div id='DemandPaging' style="font-family: myfont;">
		
		
				<el-container class="home-container">
					<el-header style="background-color:#79bbff">
						<div style="text-align: center;font-size:xx-large ;">操作系统项目02——请求调页存储管理方式模拟</div>
						<div style="text-align: right;font-size:large ;">2152085孙亦菲</div>
					</el-header>
					<el-container>
						<el-aside width="500px"  style="background-color: #c6e2ff;text-align: center;font-size:xx-large">
							
                                <p> 执行总指令数</p>
                                <p>{{SumIndex}}</p>
                            
                             <el-divider></el-divider>
							
                                <p> 页面置换算法</p>
                                  <el-select v-model="algorithmvalue" class="m-2" 
                                  size="large" :disabled="algorithmstate" ref="algorithm">
                                    <el-option
                                     v-for="item in algorithmOptions"
                                    :key="item.value"
                                    :label="item.label"
                                    :value="item.value"
                                    />
                                    </el-select>
						
                                <p> 已执行指令数</p>
                                <p>{{CurrentIndex+1}}</p>
                                
                                <p> 目前访问页面总数</p>
                                <p>{{AccessSum}}</p>
                                
                                <p>缺页次数</p>
                                <p>{{AccessFail}}</p>
                                
                                <p>目前缺页率</p>
                                <p>{{AccessFail*100/AccessSum}}%</p>
							
						</el-aside>
						
						<el-main> 
							<MemoryShow ref="MemoryShow"/>
                            
                            <el-row  >
                                <el-col :span="10" >
                                    <InstructionTable ref="InstructionTable"/>
                                </el-col>
                                <el-col :span="8"  >
                                   <div class="button-container">
                                        <el-button round type="primary" :disabled="nextstate" size="large" v-on:click="nextStep()">单步执行</el-button>
                                        <el-button round type="info"  size="large" v-on:click=" keepStep()">连续执行</el-button>
                                        <el-button round type="warning" :disabled="restate" size="large"  v-on:click="restart()">重置</el-button>
                                    </div>
                                </el-col>
                            </el-row>
                             
						</el-main>
					</el-container>
				</el-container>
		
	
	</div>
 </template>
 
 <style scoped>
    .button-container {
        margin-top: 25%;
        display: flex;
        flex-direction: column;
        align-items: center;
        justify-content: center;
        gap: 20px; /* 按钮之间的空隙大小 */
}
</style>

<script>
	import MemoryShow from "@/components/MemoryShow";
    import InstructionTable from "@/components/InstructionTable"
    import { ElMessage } from 'element-plus'
    
    
	export default {
		name: 'App',
		components: {
		MemoryShow,
        InstructionTable
		},
            
        data:()=>
        ({
            algorithmstate:false,
            nextstate:false,
            restate:false,
            
            algorithmOptions:
            [
                {
                    value: 'FIFO',
                    label: '先进先出算法（FIFO）',
                },
                {
                    value: 'LRU',
                    label: '最近最久未使用（LRU）',
                },
                
            ],
            algorithmvalue:'FIFO',
            
            CurrentIndex:-1,
            SumIndex:320,
            InstructionList:[],//指令执行顺序列表
            FrameNum:4,
            InnerFrame:[],//在内存中的页
            FIFOPointer:-1,//最先进页面的指针
            FIFOList:[],//调进内存页面的内存块记录
            LRUList:[],//每块上次使用时间间隔
            AccessSum:0,
            AccessFail:0,
            timer:'',
            keeptimer:0,
        }),
        methods:{
        restart()
        {
            this.AccessSum=0;
            this.AccessFail=0;
            
            
            this.CurrentIndex=-1;
            
            this.$refs.InstructionTable.clear();
            this.$refs.MemoryShow.clear();
            
            this.FIFOPointer=-1;
        },
        //初始化
        init() 
        {
            this.LRUList.splice(0);
            
            this.FIFOList.splice(0);
            
            this.InstructionList.splice(0);
            for(let i=0;i<this.FrameNum;i++)
            {
                this.LRUList.push(-1);
            }
            
            this.InnerFrame.splice(0);
            for(let i=0;i<this.FrameNum;i++)
            {
                this.InnerFrame.push(-1);
            }
            
            let InstructionDone=new Array(this.SumIndex).fill(0);
            let count=-1;
            let m=Math.floor(Math.random()*(this.SumIndex-1));

            
            count++;
            this.InstructionList.push(m);
            InstructionDone[m]=1;
            console.log("m "+m);
            
            count++;
            this.InstructionList.push(m+1);
            InstructionDone[m+1]=1;
             console.log("m+1 "+(m+1));
            let index=0;
            
            let findPos=(left,right)=>
            {
                let flag=0;
               // console.log(left,right);
                for(let i=left;i<=right;i++)
                {
                    if(!InstructionDone[i])
                    {
                        flag=1;
                        break;
                    }
                }
                if(flag===0)
                {
                    //console.log("done")
                    return [0,-1];
                }
                 do
                 {
                    index=left+Math.floor(Math.random()*(right-left)); 
                 }while(InstructionDone[index]);
                // console.log("index "+index)
                 return [1,index];
                 
            }
            
             let flag,m1,m11,m2,m22;
            while(count<this.SumIndex)
            {
                
                //通过随机数，跳转到前地址部分0~$(m-1)$中的某个指令处，
                //其序号为m1
                
                 //若前面已经没有尚未执行的指令，则在全局范围内产生随机数，
                 //直到找到一个还未执行的指令
                count++;
                if(count>=this.SumIndex)
                {
                    break;
                }
                [flag,m1]=findPos(0,m-1);
                if(flag===0)
                {
                    [flag,m1]=findPos(0,this.SumIndex-1);
                }
                 this.InstructionList.push(m1);
                 InstructionDone[m1]=1;
                // console.log("m1 "+m1);
                 //顺序执行后面第一条未执行的指令，即序号为m1+n的指令
                 count++;
                
                 if(count>=this.SumIndex)
                 {
                     break;
                 }
                for(let i=m1+1;i<this.SumIndex;i++)
                {
                    if(InstructionDone[i]==0)
                    {
                        m11=i;
                        flag=1;
                        break;
                    }
                }
                this.InstructionList.push(m11);
                InstructionDone[m11]=1;
               // console.log("m11 "+m11);
                //通过随机数，跳转到后地址部分$(m_1+n+1)$~319中的某条指令处，
                //其序号为$m_2$
             //若后面已经没有尚未执行的指令，则在全局范围内产生随机数，直到找到一个还未执行的指令
                count++;
                if(count>=this.SumIndex)
                {
                    break;
                }
                [flag,m2]=findPos(m11+1,m-1);
               // console.log(flag)
                if(flag===0)
                {
                    [flag,m2]=findPos(0,this.SumIndex-1);
                }
                 this.InstructionList.push(m2);
                 InstructionDone[m2]=1;
                // console.log("m2 "+m2);
                //顺序执行后面第一条未执行的指令，即序号为$m_2+n$的指令
                count++;
                if(count>=this.SumIndex)
                {
                    break;
                }
                for(let i=m2+1;i<this.SumIndex;i++)
                {
                    if(! InstructionDone[i])
                    {
                        m22=i;
                        flag=1;
                        break;
                    }
                }
                this.InstructionList.push(m22);
                InstructionDone[m22]=1;
                //console.log("m22 "+m22);
            }
            
        },
        checkEnd()
        {
            if(this.CurrentIndex>=this.SumIndex-1)
            {
                ElMessage.success({
                  message: '全部320条指令已执行完毕',
                  type: 'success',
                });
                this.algorithmstate=false;
                this.nextstate=false;
                this.restate=false;
                 clearInterval(this.timer);
                return 1;
            }
            return 0;
        },
        nextStep() 
        {
             
            let Instruction={};
            
            if(this.CurrentIndex==-1)
            {
                this.init();
            }
             
            
            if(this.checkEnd()==1)
            {
                console.log("over");
                
                return ;
            }
             this.CurrentIndex++;
            let inpage= Math.floor(this.InstructionList[this.CurrentIndex]/10);
            
            let exist=0;
            let SelectedFrame=-1;
            
            this.AccessSum++;
            for(let i=0;i<this.FrameNum;i++)
            {
                if(inpage==this.InnerFrame[i])
                {
                    exist=1;
                    SelectedFrame=i;
                    break;
                }
            }
            Instruction['index']=this.CurrentIndex;
            Instruction['place']=this.InstructionList[this.CurrentIndex];
            if(exist==1)
            {
                Instruction['loss']="否";
                Instruction['outpage']='-';
                Instruction['inpage']='-';
            }
            else
            {
                 
                this.AccessFail++;
                Instruction['loss']="是";
                Instruction['inpage']=inpage;
                
                for(let i=0;i<this.FrameNum;i++)
                {
                    if(this.InnerFrame[i]==-1)
                    {
                       SelectedFrame=i;
                        break;
                    }
                }
                if(SelectedFrame==-1)//没有空闲框
                {
                     console.log(this.algorithmvalue);
                    if(this.algorithmvalue=='FIFO')
                    {
                         SelectedFrame=this.FIFO();
                         console.log("FIFO");
                    }
                    else
                    {
                        SelectedFrame=this.LRU();
                        console.log("LRU");
                    }
                    
                    console.log(SelectedFrame);
                    Instruction['outpage']=this.InnerFrame[SelectedFrame];
                    
                }
                else//有空闲框
                {
                     Instruction['outpage']='-';
                }
                this.FIFOList.push(SelectedFrame);
                
                this.InnerFrame[SelectedFrame]=inpage;
            }
            console.log(Instruction);
            
            for(let i=0;i<this.FrameNum;i++)
            {
                if(this.LRUList[i]!=-1)
                {
                    this.LRUList[i]++;
                }
            }
            this.LRUList[SelectedFrame]=0;
            //展示选择页框中新页
            console.log(`MemoryBlock${SelectedFrame}`);
            this.$refs.MemoryShow.showInPage(SelectedFrame,inpage,this.InstructionList[this.CurrentIndex]%10);
            //加入新数据
            this.$refs.InstructionTable.addData(Instruction);
        },
        keepStep()
        {
          this.timer=setInterval(this.nextStep,20);             
         this.algorithmstate=true;
         this.nextstate=true;
         this.restate=true;
        },
        FIFO()
        {
            ++this.FIFOPointer;
            
            return this.FIFOList[this.FIFOPointer];
        },
        LRU()
        {
            
            let index=0;
            let maxTime=this.LRUList[index];
            for(let i=1;i<this.FrameNum;i++)
            {
                if(this.LRUList[i]>maxTime)
                {
                    maxTime=this.LRUList[i];
                    index=i;
                }
            }
            return index;
        }
        }
        
    }
</script>