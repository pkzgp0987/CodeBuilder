//
//  _ModelName_Query.cpp
//
//  Created by 曾贵平 on _time_.
//

#include "_ModelName_Query.h"

static _ModelName_Query* pQuery=NULL;

_ModelName_Query* _ModelName_Query::GetQuery()
{
	if (pQuery==NULL)
	{
		pQuery=new _ModelName_Query();
	}
	return pQuery;
}

void _ModelName_Query::Insert(_ModelName_ *pEntity)
{
    char* str= ( char* )malloc(1024);
    sprintf(str,"insert into _TableName_(<#foreach[table]case[0]:[_columnName_](,)#>) values (<#foreach[table]case[INTEGER,REAL,TEXT]:['%d';'%f';'%s'](,)#>)",<#foreach[table]case[INTEGER,REAL,TEXT]:[pEntity->_columnName_;pEntity->_columnName_;pEntity->_columnName_.c_str()](,)#>);
	DBConnection::GetConnection()->ExecSQL(str,NULL,NULL);
	pEntity->ID=DBConnection::GetConnection()->GetInsertID();
	delete str;
}

void InsertWithID(_ModelName_ *pEntity)//插入一行数据
{
    char* str= ( char* )malloc(1024);
    sprintf(str,"insert into _TableName_(<#foreach[table0]case[0]:[_columnName_](,)#>) values (<#foreach[table0]case[INTEGER,REAL,TEXT]:['%d';'%f';'%s'](,)#>)",<#foreach[table0]case[INTEGER,REAL,TEXT]:[pEntity->_columnName_;pEntity->_columnName_;pEntity->_columnName_.c_str()](,)#>);
	DBConnection::GetConnection()->ExecSQL(str,NULL,NULL);
	delete str;
}

void _ModelName_Query::Update(_ModelName_ *pEntity)
{
	char* str= ( char* )malloc(1024);
    sprintf(str,"update _TableName_ set <#foreach[table]case[INTEGER,REAL,TEXT]:[_columnName_='%d';_columnName_='%f';_columnName_='%s'](,)#> where <#PIDcase[INTEGER,REAL,TEXT]:[_PID_='%d';_PID_='%f';_PID_='%s']#>",<#foreach[table0]case[INTEGER,REAL,TEXT]:[pEntity->_columnName_;pEntity->_columnName_;pEntity->_columnName_.c_str()](,)#>,<#PIDcase[INTEGER,REAL,TEXT]:[pEntity->_PID_;pEntity->_PID_;pEntity->_PID_.c_str()]#>);
	DBConnection::GetConnection()->ExecSQL(str,NULL,NULL);
	delete str;
}

void _ModelName_Query::DeleteByID(<#PIDcase[INTEGER,REAL,TEXT]:[int;float;const char*]#> ID)
{
	char* str= ( char* )malloc(1024);
    sprintf(str,"delete from _TableName_ where <#PIDcase[INTEGER,REAL,TEXT]:[_PID_='%d';_PID_='%f';_PID_='%s']#>",ID);
	DBConnection::GetConnection()->ExecSQL(str,NULL,NULL);
	delete str;
}

int _ModelName_SelectCB( void * para, int n_column, char ** column_value, char ** column_name )
{
	_ModelName_ *pEntity=(_ModelName_ *)para;
    <#foreach[table0]case[INTEGER,REAL,TEXT]:[sscanf(column_value[_i_],"%d",&pEntity->_columnName_);sscanf(column_value[_i_],"%f",&pEntity->_columnName_);pEntity->_columnName_=string(column_value[_i_])](;nT)#>;
	return 0;
}

_ModelName_ *_ModelName_Query::SelectByID(<#PIDcase[INTEGER,REAL,TEXT]:[int;float;const char*]#> ID)
{
	_ModelName_ *pEntity=new _ModelName_();
	char* str= ( char* )malloc(1024);
    sprintf(str,"select * from _TableName_ where <#PIDcase[INTEGER,REAL,TEXT]:[_PID_='%d';_PID_='%f';_PID_='%s']#>",ID);
	DBConnection::GetConnection()->ExecSQL(str,_ModelName_SelectCB,pEntity);
	delete str;
	return pEntity;
}

int _ModelName_SelectArrayCB( void * para, int n_column, char ** column_value, char ** column_name )
{
	std::list<_ModelName_*> *pArray=(std::list<_ModelName_*> *)para;
	_ModelName_ *pEntity=new _ModelName_;
	<#foreach[table0]case[INTEGER,REAL,TEXT]:[sscanf(column_value[_i_],"%d",&pEntity->_columnName_);sscanf(column_value[_i_],"%f",&pEntity->_columnName_);pEntity->_columnName_=string(column_value[_i_])](;nT)#>;
	pArray->push_back(pEntity);
	return 0;
}
std::list<_ModelName_*> SelectByWhere(const char* where)//查找所有数据
{
    std::list<_ModelName_*> pArray;
	char* str= ( char* )malloc(1024);
    sprintf(str,"select * from _TableName_ where %s",where);
	DBConnection::GetConnection()->ExecSQL(str,_ModelName_SelectArrayCB,&pArray);
	delete str;
	return pArray;
}
std::list<_ModelName_*> _ModelName_Query::SelectAll()
{
	std::list<_ModelName_*> pArray;
	const char* str="select * from _TableName_";
	DBConnection::GetConnection()->ExecSQL(str,_ModelName_SelectArrayCB,&pArray);s
	return pArray;
}

_ModelName_Query::_ModelName_Query(void)
{
}


_ModelName_Query::~_ModelName_Query(void)
{
}
