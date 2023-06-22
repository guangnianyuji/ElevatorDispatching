import sys
import os
import pickle
from PyQt5.Qt import *

class Block:
    """
    磁盘中的一个个物理块
    """

    def __init__(self, blockSize,blockIndex, data=""):
        # 编号
        self.blockIndex = blockIndex
        # 数据
        self.data = data
        #隐式链接下一个盘号
        self.nextIndex=-1

        self.blockSize=blockSize

    def setNext(self,index):
        self.nextIndex=index
    def getNext(self):
        return self.nextIndex
    def write(self, newData):
        self.data = newData[:self.blockSize]
        return newData[self.blockSize:]

    def read(self):
        return self.data

    def isFull(self):
        return len(self.data) == self.blockSize

    def append(self, newData):
        """
        追加新内容，返回无法写入的部分
        """
        remainSpace = self.blockSize - len(self.data)
        self.data += newData[:remainSpace]
        if remainSpace >= len(newData):
            return ""
        else:
            return newData[remainSpace:]

    def clear(self):
        self.data = ""

#文件系统管理
class FileSystemManagement:
    ##空闲表
    def __init__(self,blockNum):
        self.FreeList=[]
        self.blockNum=blockNum
        for i in range(blockNum):
            self.FreeList.append(0)
        self.Catalog=dict()
    #找空闲块
    def findBlank(self):
        for i in range(self.blockNum):
            if self.FreeList[i]==0:
                return i
        return -1

    #写入文件的数据、名称
    def write(self,data,disk,name):
        last=-1

        if(self.Catalog.get(name)!=None):
            last=self.Catalog.get(name)[1]

        while data!="":
            cur=self.findBlank()
            if(cur==-1):
                raise Exception(print('磁盘空间不足!'))

            else:
                if(last == -1):
                    self.Catalog[name]=[cur,-1]
                else:
                    disk[last].setNext(cur)#指针指下一个
                data=disk[cur].write(data)#写入
                self.FreeList[cur]=1#空闲表

                last=cur#上一个

        self.Catalog[name][1]=last
        disk[last].setNext(-1)

    def delete(self,disk,name):
        if self.Catalog.get(name)==None:
            self.Catalog[name]=[-1,-1]

        cur=self.Catalog[name][0]

        while(cur!=-1):
            disk[cur].clear()
            self.FreeList[cur]=0
            cur=disk[cur].getNext()


    def read(self,name,disk):
        data=""
        if self.Catalog.get(name)==None:
            self.Catalog[name]=[-1,-1]

        cur=self.Catalog[name][0]

        while(cur!=-1):
            data+=disk[cur].read()
            cur=disk[cur].getNext()
        return data

    def update(self,data,name,disk):
         self.delete(disk,name)
         self.write(data,disk,name)
         return self.Catalog[name][0]

class FCB:
    def __init__(self, name_url):
        # 文件名
        self.name = name_url
        self.add=-1
        self.len=0
    def read(self,manager,disk):

        return manager.read(self.name,disk)
    def update(self,data,manager,disk):
         self.add=manager.update(data,self.name,disk)
         self.len=len(data)
    def delete(self,manager,disk):

         manager.delete(disk,self.name)

class CatalogNode:
    """
    多级目录结点
    """
    def __init__(self, name,type,parent,url):
        # 路径名
        self.name=name
        self.type=type

        self.parent=parent
        self.url=url

        if type=='file':
            self.data=FCB(url)
        else:
            self.children=[]

from PyQt5.QtWidgets import QWidget
class editForm(QWidget):


    def __init__(self, name, data,par):
        super().__init__()

        self.par=par
        self.setWindowTitle(name)
        self.name = name
        self.setWindowIcon(QIcon('ico/编辑.png'))

        self.resize(750, 400)
        self.text_edit = QTextEdit(self)  # 实例化一个QTextEdit对象
        self.text_edit.setText(data)  # 设置编辑框初始化时显示的文本
        self.text_edit.setPlaceholderText("在此输入文件内容")  # 设置占位字符串

        self.initialData = data

        self.save_button = QPushButton("Save", self)
        self.clear_button = QPushButton("Clear", self)

        self.save_button.clicked.connect(lambda: self.button_slot(self.save_button))
        self.clear_button.clicked.connect(lambda: self.button_slot(self.clear_button))

        self.h_layout = QHBoxLayout()
        self.v_layout = QVBoxLayout()

        self.h_layout.addWidget(self.save_button)
        self.h_layout.addWidget(self.clear_button)
        self.v_layout.addWidget(self.text_edit)
        self.v_layout.addLayout(self.h_layout)

        self.setLayout(self.v_layout)


    def button_slot(self, button):
        if button == self.save_button:

            choice = QMessageBox.question(self, "Question", "Do you want to save it?", QMessageBox.Yes | QMessageBox.No)
            if choice == QMessageBox.Yes:
                self.par.updateData(self.text_edit.toPlainText())
                self.par.setEnabled(True)
                self.close()
            elif choice == QMessageBox.No:
                self.par.setEnabled(True)
                self.close()
        elif button == self.clear_button:
            self.text_edit.clear()


    def closeEvent(self, event):
        self.par.setEnabled(True)


class FileManagementWindow(QMainWindow):
    def __init__(self):
        super().__init__()#调用父类的init方法

        self.blockSize=512
        self.blockNum=512
        self.resize(1500, 927)
        self.setWindowTitle('操作系统项目03——文件系统管理项目  2152085孙亦菲')
        self.setWindowIcon(QIcon('ico/ico.jpg'))

        #工具栏
        MyToolBar=self.addToolBar("MyToolBar")

        #新建文件夹
        NewFolderAction=QAction(QIcon('ico/folder.png'),"NewFolder",self)
        NewFolderAction.triggered.connect(self.createFolder)
        MyToolBar.addAction(NewFolderAction)
        #新建文件
        NewFileAction = QAction(QIcon('ico/file.png'), "NewFile", self)
        NewFileAction.triggered.connect(self.createFile)
        MyToolBar.addAction(NewFileAction)
        #编辑文件
        EditAction=QAction(QIcon("ico/编辑.png"),"Edit",self)
        EditAction.triggered.connect(self.edit)
        MyToolBar.addAction( EditAction)
        #删除文件或文件夹
        DeleteAction=QAction(QIcon("ico/删除.png"), "Delete", self)
        DeleteAction.triggered.connect(self.delete)
        MyToolBar.addAction(DeleteAction)
        #格式化
        FormatAction = QAction(QIcon("ico/格式刷.png"), "Format", self)
        FormatAction.triggered.connect(self.format)
        MyToolBar.addAction( FormatAction)
        MyToolBar.addSeparator()


        self.url=">root"
        #当前地址
        self.curAddress=QLineEdit()
        self.curAddress.addAction(QIcon('ico/文件夹.png'),QLineEdit.LeadingPosition)
        self.curAddress.setText(self.url)
        self.curAddress.setReadOnly(True)

        MyToolBar.addWidget(self.curAddress)

        #窗口内容
        grid=QGridLayout()

        self.initial()

        self.MyWidGet=QWidget()#我的组件
        self.MyWidGet.setLayout(grid)
        self.setCentralWidget(self.MyWidGet)

        #目录树
        self.DirTree=QTreeWidget()
        self.DirTree.setHeaderLabels(['Name', 'Type','Address','Length'])
        self.rootItem=self.buildTree(self.root,self.DirTree)#建立这棵树
        grid.addWidget(self.DirTree)

        #设置选中状态
        self.DirTree.setCurrentItem(self.rootItem)
        #设置当前路径
        self.path=[self.rootItem]
        #绑定单击事件
        self.DirTree.itemClicked['QTreeWidgetItem*','int'].connect(self.clickTreeItem)
        #
        self.curNode=self.root

    def format(self):
        reply = QMessageBox.critical(self, "格式化不可恢复", "确认格式化？", QMessageBox.Yes | QMessageBox.No,
                                    QMessageBox.Yes)

        if reply == QMessageBox.No:
            return

        parent=self.root
        #删目录节点
        self.deleteCatalogNode(parent)

        self.catalog.append(self.root)


        #删树形结构
        self.deleteTree(self.rootItem)

        self.curNode=self.root
        self.DirTree.setCurrentItem(self.rootItem)
        self.clickTreeItem(self.rootItem)

    def clickTreeItem(self,item:QTreeWidgetItem):
        self.url=""
        ways=[item]
        temp=item

        while(temp!=self.rootItem):

            temp=temp.parent()
            ways.append(temp)


        ways.reverse()
        self.curNode=self.root

        for i in ways:
            self.url=self.url+">"+i.text(0)
            if i==self.rootItem:
                continue


            for node in self.curNode.children:
                if node.name==i.text(0):
                    self.curNode=node
                    break

        self.curAddress.setText(self.url)

    def delete(self):
        if(self.curNode==self.root):
            QMessageBox.critical(self, "警告对话框", "磁盘不可删除！！", QMessageBox.Yes)
            return

        #提示框
        reply=QMessageBox.warning(self, "删除不可恢复", "确认删除？", QMessageBox.Yes|QMessageBox.No,QMessageBox.Yes)

        if reply==QMessageBox.No:
            return

        parent=self.curNode.parent

        parent.children.remove(self.curNode)
        self.deleteCatalogNode(self.curNode)#删目录表
        self.curNode=parent


        #删树形结构
        curItem=self.DirTree.currentItem()
        parentItem=curItem.parent()
        self.deleteTree(curItem)
        parentItem.removeChild(curItem)

        self.DirTree.setCurrentItem(parentItem)
        self.clickTreeItem(self.DirTree.currentItem())  # 更新当前路径

    def deleteCatalogNode(self,ParentNode:CatalogNode):
         self.catalog.remove(ParentNode)

         if ParentNode.type!="file":
             count=len(ParentNode.children)

             for i in range(count):
                 self.deleteCatalogNode(ParentNode.children[0])
                 ParentNode.children.remove(ParentNode.children[0])
         else:

             ParentNode.data.delete(self.manager,self.disk)



    def deleteTree(self,ParentItem:QTreeWidgetItem):
        count = ParentItem.childCount()
        for i in range(count):
            self.deleteTree(ParentItem.child(0))
            ParentItem.removeChild(ParentItem.child(0))

    def createFile(self):

        if(self.curNode.type=="file"):
            QMessageBox.warning(self, "警告对话框", "文件下不可新建文件", QMessageBox.Yes)
            return

        value, ok = QInputDialog.getText(self, "新建文件", "请输入文件名:", QLineEdit.Normal,
                                         "file")
        if ok==True:
            for node in self.curNode.children:
                if node.name==value:
                    QMessageBox.warning(self, "警告对话框", "文件名重复！新建失败", QMessageBox.Yes)
                    return

            newNode=CatalogNode(value,"file",self.curNode,self.url+">"+value)
            self.curNode.children.append(newNode)
            self.catalog.append(newNode)#最后在目录加新节点

            parent=self.DirTree.currentItem()
            child = QTreeWidgetItem(parent)

            self.createTreeNode(child,newNode)

    def createFolder(self):
        if(self.curNode.type=="file"):
            QMessageBox.warning(self, "警告对话框", "文件下不可新建文件夹", QMessageBox.Yes)
            return

        value, ok = QInputDialog.getText(self, "新建文件夹", "请输入文件夹名:", QLineEdit.Normal,
                                         "folder")
        if ok==True:
            for node in self.curNode.children:
                if node.name==value :
                    QMessageBox.warning(self, "警告对话框", "文件名重复！新建失败", QMessageBox.Yes)
                    return
            newNode=CatalogNode(value,"folder",self.curNode,self.url+">"+value)
            self.curNode.children.append(newNode)
            self.catalog.append(newNode)

            parent=self.DirTree.currentItem()
            child = QTreeWidgetItem(parent)
            self.createTreeNode(child,newNode)
    def edit(self):
        if (self.curNode.type != "file"):
            QMessageBox.warning(self, "警告对话框", "非文件不可编辑", QMessageBox.Yes)
            return

        data=self.curNode.data.read(self.manager,self.disk)

        editChild=editForm(self.url,data,self)
        self.setEnabled(False)
        editChild.show()

    def updateData(self,content):
        self.curNode.data.update(content,self.manager,self.disk)
        self.DirTree.currentItem().setText(2,str(self.curNode.data.add))
        self.DirTree.currentItem().setText(3, str(self.curNode.data.len))

    def createTreeNode(self,child,node):
        child.setText(0, node.name)
        child.setText(1, node.type)
        if node.type=='file':

            child.setText(2,str(node.data.add))
            child.setText(3, str(node.data.len))

        if node.type == "disk":
            child.setIcon(0, QIcon('ico/磁盘.png'))
        elif node.type == "folder":
            child.setIcon(0, QIcon('ico/folder.png'))
        elif node.type == "file":
            child.setIcon(0, QIcon('ico/file.png'))
    def buildTree(self,node,parent):
        child=QTreeWidgetItem(parent)
        self.createTreeNode(child,node)

        if not node.type=="file":
            for childnode in node.children:
                self.buildTree(childnode,child)
        return child#返回新创造的节点
    def initial(self):
        if not os.path.exists('manager'):
            self.manager=FileSystemManagement(self.blockNum)


        else:
            with open('manager','rb') as f:
                self.manager=pickle.load(f)
        #读取catalog表
        if not os.path.exists('catalog'):
            self.catalog=[]
            self.root=CatalogNode("root", "disk",None,self.url)
            self.catalog.append(self.root)

        else:
            with open('catalog','rb') as f:
                self.catalog=pickle.load(f)
                self.root=self.catalog[0]
        #读取disk表
        if not os.path.exists('disk'):
            self.disk=[]
            for i in range(self.blockNum):
                self.disk.append(Block(self.blockSize,i))
        else:
            with open('disk','rb') as f:
                self.disk=pickle.load(f)

    def closeEvent(self, event):
        with open('manager', 'wb') as f:
            f.write(pickle.dumps(self.manager))
        # 存储
        with open('catalog', 'wb') as f:
            f.write(pickle.dumps(self.catalog))

        # 存储disk表
        with open('disk', 'wb') as f:
            f.write(pickle.dumps(self.disk))

if __name__=='__main__':
    app = QApplication(sys.argv)
    mainform=FileManagementWindow()
    mainform.show()
    sys.exit(app.exec_())