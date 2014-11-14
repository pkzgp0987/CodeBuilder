//
//  DBConnection.h
//  LongLand
//
//  Created by 曾贵平 on 13-12-20.
//
//
#ifndef __DBConnection_H__
#define __DBConnection_H__

#include <sys/socket.h> // Per msqr
#include <sys/sysctl.h>
#include <net/if.h>
#include <net/if_dl.h>

#include "sqlite3.h"
#include "cocos2d.h"
#include "stdio.h"
#include "GameDefine.h"

class DBConnection
{
public:
	bool CheckInit(); //检查是否初始化
    void InitDB(); //初始化
	void DeleteDB(); //删除数据库
	bool ExecSQL(char * sql_command, int (*callback)(void*,int,char**,char**), void *); //执行SQL
	bool ExecSQL(const char * sql_command, int (*callback)(void*,int,char**,char**), void *); //执行SQL
	static DBConnection* GetConnection(string sDBFileName=DB_FILE_NAME); //单例模式
    static DBConnection* GetLogConnection(string sDBFileName=LOG_DB_FILE_NAME); //单例模式
    static DBConnection* GetRoleConnection(string sDBFileName=Role_DB_FILE_NAME); //单例模式
	int GetInsertID();
private:
	sqlite3 *pDB;//数据库指针
	DBConnection(void);
	~DBConnection(void);
    const char* DeSqlFile(const char* cfileName);
    cocos2d::CCString* getMacCCString();
    char m_sMac[4]={'z','x','p','l'};
    void MoveDBFile(string dbName=Role_DB_FILE_NAME);
    string m_DBFileName;
};
#endif
