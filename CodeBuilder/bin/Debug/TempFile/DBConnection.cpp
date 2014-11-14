//
//  DBConnection.cpp
//
//  Created by 曾贵平 on 13-12-20.
//
//
#include "DBConnection.h"

using namespace cocos2d;

static DBConnection* pConnection = NULL;

DBConnection* DBConnection::GetConnection(string sDBFileName)
{
	if (pConnection == NULL)
	{
		pConnection =  new DBConnection();
        pConnection->m_DBFileName=sDBFileName;
	}

	return pConnection;
}

static DBConnection* pLogConnection = NULL;

DBConnection* DBConnection::GetLogConnection(string sDBFileName) //单例模式
{
    if (pLogConnection == NULL)
	{
		pLogConnection =  new DBConnection();
        pLogConnection->m_DBFileName=sDBFileName;
	}
	return pLogConnection;
}

static DBConnection* pRoleConnection = NULL;

DBConnection* DBConnection::GetRoleConnection(string sDBFileName) //单例模式
{
    if (pRoleConnection == NULL)
	{
		pRoleConnection =  new DBConnection();
        pRoleConnection->m_DBFileName=sDBFileName;
	}
    
	return pRoleConnection;
}

bool DBConnection::CheckInit()
{
	return true;
}

void DBConnection::DeleteDB()
{
	//const char * dbpath = CCFileUtils::sharedFileUtils()->fullPathFromRelativePath(DB_FILE_NAME);
    CCFileUtils::sharedFileUtils()->getWritablePath();
    const char * dbpath = CCString::create(CCFileUtils::sharedFileUtils()->getWritablePath()+m_DBFileName)->getCString();
    CCLOG("dbpath = %s",dbpath);
    remove(dbpath);
    pDB = NULL;
}

void DBConnection::InitDB()
{
//    DeleteDB();  //删除数据库
    
	//检查DB是否存在
//	const char * dbpath = CCFileUtils::sharedFileUtils()->fullPathFromRelativePath(DB_FILE_NAME);
    const char * dbpath = CCString::create(CCFileUtils::sharedFileUtils()->getWritablePath()+m_DBFileName)->getCString();
    CCLOG("dbpath = %s",dbpath);
	int check = access(dbpath,0);
  
	//建库
	if (check!=0)
	{
        //解密数据库文件
        //const char* sTempFileName=DeSqlFile(CCFileUtils::sharedFileUtils()->fullPathForFilename(SQL_FILE_NAME).c_str());
        //CCString *pStr = CCString::createWithContentsOfFile(sTempFileName);
        
        //20130825 EdifierWill
        //读取TXT建库
//        ExecSQL("PRAGMA auto_vacuum = 1",NULL,NULL);//开启删除后自动收缩数据文件大小(1为开启,0为关闭)
//		CCString *pStr = CCString::createWithContentsOfFile(CCFileUtils::sharedFileUtils()->fullPathForFilename(SQL_CREATE_NAME).c_str());
//        //remove(sTempFileName);
//		ExecSQL(pStr->getCString(),NULL,NULL);
//
//        pStr=CCString::createWithContentsOfFile(CCFileUtils::sharedFileUtils()->fullPathForFilename(SQL_INSERT_NAME).c_str());
//        ExecSQL(pStr->getCString(),NULL,NULL);
//        
//        /*------曾贵平修改14.5.15--------*/
//        //        delete pStr;
//        My_SAFE_RELEASE(pStr);
        /*------开启多线程写数据库时delete会报错所以修改---------*/
        //直接复制库文件
        if(strcmp(m_DBFileName.c_str(), LOG_DB_FILE_NAME)==0)
        {
            MoveDBFile(LOG_DB_FILE_NAME);
        }
        else if(strcmp(m_DBFileName.c_str(), DB_FILE_NAME)==0)
        {
            MoveDBFile(DB_FILE_NAME);
        }
        else
        {
            MoveDBFile();
        }
	}
}

DBConnection::DBConnection(void)
{
	pDB = NULL;
    std::string strMac=getMacCCString()->getCString();
    for(int i =0;i<4;i++)
    {
        m_sMac[i]=strMac[(4+(i*2))];
    }
}

int DBConnection::GetInsertID(void)
{
	sqlite3_int64 ID = sqlite3_last_insert_rowid(pDB);
	return (int)ID;
}

bool DBConnection::ExecSQL(const char * sql_command,int (*callback)(void*,int,char**,char**),void * para)
{
	int size = strlen(sql_command);
	char * temp = new char[size+1];
	temp[size]='\0';
	sprintf(temp,sql_command);
//    CCLog("%s",sql_command);
	return ExecSQL(temp, callback, para);
}

bool DBConnection::ExecSQL(char * sql_command,int (*callback)(void*,int,char**,char**),void * para)
{
	if (pDB == NULL)
	{
		//const char * dbpath = CCFileUtils::sharedFileUtils()->fullPathFromRelativePath(DB_FILE_NAME);
        const char * dbpath = CCString::create(CCFileUtils::sharedFileUtils()->getWritablePath()+m_DBFileName)->getCString();
//        const char * dbpath = "/Users/maliao/Documents/LongLand/projects/LongLand/Resources/GameDB.sqlite";
		int result = sqlite3_open(dbpath, &pDB);
        //添加密码
        //result = sqlite3_key(pDB, pConnection->m_sMac, 4);
        //result = sqlite3_key(pDB, "wllp", 4);
		if( result != SQLITE_OK ) 
		{
			CCLOG( "open db error:%d" , result);
            delete []sql_command;
			return false;
		}
	}
	char * errMsg = NULL;//错误信息
    
    //解密读取
    //sqlite3_key(pDB, pConnection->m_sMac, 4);
    //sqlite3_key(pDB, "wllp", 4);
	int result=sqlite3_exec( pDB, sql_command , callback, para, &errMsg );
	if( result != SQLITE_OK ) 
	{
		CCLog( "SQL exec error:%d, reason:%s" , result, errMsg );
        CCLog("sql----%s",sql_command);
        delete []sql_command;
		return false;
	}
	else
    {
        delete []sql_command;
		return true;
    }
}

DBConnection::~DBConnection(void)
{
	//关闭数据库 
	sqlite3_close(pDB); 
}

const char* DBConnection::DeSqlFile(const char* sFileName)
{
    int i,count,len;
	char buff[1024];
	char* Enkey="HunterLengend4EdifierWill";
    const char* tmpfile= CCString::create(CCFileUtils::sharedFileUtils()->getWritablePath().append("~u~0_sw~.f~l"))->getCString();//临时文件储存;
   
	FILE * in , * out ;
    
	len =strlen(Enkey);
	in =fopen(sFileName,"rb");
	out =fopen(tmpfile,"wb"); /* creat a temp file */
    
	if(in==NULL)
	{
		printf("Input File \'%s\' not found !!\n",sFileName);
		exit(1);
	}
	if(out==NULL)
	{
		printf("Can not creat temp file \'%s\'\n",tmpfile);
		exit(2);
	}
    
	while( !feof(in) )
	{
		count=fread(buff,1,1024,in);
		for(i=0;i <count;i++)
			buff[i]^=Enkey[(i+i/3)%len];
		fwrite(buff,1,count,out);
	}
    
	fclose(in);
	fclose(out);
	//remove(sFileName);
	//rename(tmpfile,sFileName);
    
    return tmpfile;
}

cocos2d::CCString* DBConnection::getMacCCString()
{
    int                   mib[6];
    size_t                len;
    char                  *buf;
    unsigned char         *ptr;
    struct if_msghdr      *ifm;
    struct sockaddr_dl    *sdl;
    
    mib[0] = CTL_NET;
    mib[1] = AF_ROUTE;
    mib[2] = 0;
    mib[3] = AF_LINK;
    mib[4] = NET_RT_IFLIST;
    
    if ((mib[5] = if_nametoindex("en0")) == 0) {
        CCLOG("Error: if_nametoindex error/n");
    }
    
    if (sysctl(mib, 6, NULL, &len, NULL, 0) < 0) {
        CCLOG("Error: sysctl, take 1/n");
    }
    
    if ((buf = (char*)malloc(len)) == NULL) {
        CCLOG("Could not allocate memory. error!/n");
    }
    
    if (sysctl(mib, 6, buf, &len, NULL, 0) < 0) {
        CCLOG("Error: sysctl, take 2");
    }
    
    ifm = (struct if_msghdr *)buf;
    sdl = (struct sockaddr_dl *)(ifm + 1);
    ptr = (unsigned char *)LLADDR(sdl);
    free(buf);
    CCString *strMac=CCString::createWithFormat("%02x%02x%02x%02x%02x%02x", *ptr, *(ptr+1), *(ptr+2), *(ptr+3), *(ptr+4), *(ptr+5));
    
    return strMac;
}
void DBConnection::MoveDBFile(string dbName)
{
    const char* sTempFileName=CCFileUtils::sharedFileUtils()->fullPathForFilename(CCString::createWithFormat("DB/%s",dbName.c_str())->getCString()).c_str();
    int i,count,len;
	char buff[1024];
    const char* tmpfile= CCString::create(CCFileUtils::sharedFileUtils()->getWritablePath().append(m_DBFileName))->getCString();//临时文件储存;
    
	FILE * in=NULL , * out=NULL ;
    
	//len =strlen(Enkey);
	in =fopen(sTempFileName,"rb");
	out =fopen(tmpfile,"wb"); /* creat a temp file */
    
	if(in==NULL)
	{
		printf("Input File \'%s\' not found !!\n",sTempFileName);
		exit(1);
	}
	if(out==NULL)
	{
		printf("Can not creat temp file \'%s\'\n",tmpfile);
		exit(2);
	}
    
	while( !feof(in) )
	{
		count=fread(buff,1,1024,in);
		fwrite(buff,1,count,out);
	}
    
	fclose(in);
	fclose(out);
}