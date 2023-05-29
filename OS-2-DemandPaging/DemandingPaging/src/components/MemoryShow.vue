<template>
	<div >
		<h2>分配给作业的内存展示</h2>

		<el-row>
			<el-col v-for="nx in 4" :key="nx"  span>
				<MemoryBlock :ref=" el => { if(el) divs[nx-1] = el}"/>
			</el-col>
		</el-row>
	</div>
</template>

<script>
	import MemoryBlock from "@/components/MemoryBlock";
	export default {
		name: 'MemoryShow',
		components: {
		MemoryBlock,
		},
        data:()=>({
          divs:[],
        }),
        methods:
        {
            showInPage(index,pNum,iNum)
            {
                this.divs[index].fillFrame(pNum);
                this.divs[index].showInstruction(iNum);
            },
            clear()
            {
                console.log("hhhh")
                for(let i=0;i<4;i++)
                {
                    this.divs[i].clear();
                }
            }
        }
	}


</script>

<style>

</style>