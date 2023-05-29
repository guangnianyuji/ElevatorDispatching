<template>
	<el-space wrap size="large" alignment="center"  style="text-align: center;" >
		<el-card class="box-card" style="width: 200px">
			<template #header>
				<div class="card-header">
					<span v-if="!appear">NULL</span>
					<span v-else>{{'第'+pageindex+"页"}}</span>
				</div>
			</template>
			<div v-for="o in 10" :key="o" class="text item">
			<!--#337ecc-->
			<transition name="el-fade-in">
				<div class="transition-box" 
                style="background-color:#95d475;
                border:1px solid #b3b5bb;border-radius: 4px;"
                :ref=" el => { if(el) divs[o-1] = el}">
					<span v-if="!appear">NULL</span>
					<span v-else>{{o-1}}</span>
				</div>
			</transition>
			</div>
		</el-card>
	</el-space>
</template>

<script>
 
	export default
    {
		name:"MemoryBlock",
        data:()=>
        ({
            appear:false,
            pageindex:0,
            divs:[]
        }),
        methods:
        {
            fillFrame(pageindex)
            {
                this.appear=true;
                this.pageindex=pageindex;
            },
            showInstruction(insindex)
            {
                for(let i=0;i<10;i++)
                {
                     this.divs[i].style.backgroundColor='#95d475';
                }
                this.divs[insindex].style.backgroundColor='#337ecc';
            },
            clear()
            {
                this.appear=false;
                console.log("hehr");
                for(let i=0;i<10;i++)
                {
                     this.divs[i].style.backgroundColor='#95d475';
                }
            }
        }
	}
   
</script>

<style>
</style>