#include <stdlib.h>
#include <stdio.h>
#include <math.h>

int main()
{
	int i,j=0,a[10]={
		0,0,0,0,0,0,0,0,0,0	};
	for(i=1;i<5;i++)
	{
		a[j+=2] = i ;
	}
	for(i=0;i<10;i++)
	{
		printf(" a[%d] => %d \n",i,a[i]) ;
	}
	exit (0);
}
