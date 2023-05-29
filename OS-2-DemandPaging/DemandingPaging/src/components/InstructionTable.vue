<template>
    <div >
        <h2>已执行指令展示</h2>
        <el-table :data="TableData" 
            
            highlight-current-row
            scrollbar-always-on
           
            ref="Table"
            empty-text="暂无已执行过的指令"
             height="250">
            
            <el-table-column
                prop="index"
                label=""
                width="50">
            </el-table-column>
            <el-table-column
                prop="place"
                label="地址"
                width="100">
            </el-table-column>
            <el-table-column
                prop="loss"
                label="缺页"
                width="80">
            </el-table-column>
            <el-table-column
                prop="outpage"
                label="换出页"
                width="80">
            </el-table-column>
            <el-table-column
                prop="inpage"
                label="换入页"
                width="80">
            </el-table-column>
        </el-table>
    </div>    
</template>

<script>
    export default {
      name: "InstructionTable",
      data:()=>({
        TableData:[],
         
      }),
      methods:{
        addData(newData)
        {
          this.TableData.push(newData);
          console.log("add");
          this.$refs.Table.setCurrentRow(this.TableData[this.TableData.length-1]);

        
        },
        clear()
        {
             this.TableData.splice(0);
        }
      }
    }
</script>

<style>
 
</style>