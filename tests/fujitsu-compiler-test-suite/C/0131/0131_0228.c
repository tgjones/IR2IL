#include <stdlib.h>
#include <stdio.h>
#include <math.h>

int sub1(long int ax[10]);
int sub2(long int bx[10]);
int main()
{
	long int a[10]={
		10,1,9,2,8,3,7,4,6,5	};
	int  i;
	for(i=0;i<10;i++) 
	{
		a[i]=a[i]*2;
		sub1(a);
	}
	for(i=0;i<10;i++) 
	{
		a[i]=a[i]*2;
		sub2(a);
	}
	exit (0);
}
int sub1(long int ax[10])          
{
	long int b[10];
	int  i,j=0;
	for(i=0;i<10;i++)
	{
		if(ax[i]>15)
		{
			b[j]=ax[i];
			j++;
		}
	}
	for(i=0;i<j;i++)
	{
		printf("b[%ld] => %d \n",i,b[i]);
	}
	return 0;
}
int sub2(long int bx[10])          
{
	long int c[10]={
		0,0,0,0,0,0,0,0,0,0	};
	int  i,j=0;
	for(i=0;i<10;i++)
	{
		if(bx[i]>5)
		{
			c[i]=bx[j];
			j++;
		}
	}
	for(i=0;i<10;i++)
	{
		printf("c[%ld] => %d \n",i,c[i]);
	}
	return 0;
}
