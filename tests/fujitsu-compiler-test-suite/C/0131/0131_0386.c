#include <stdlib.h>
#include <stdio.h>
#include <math.h>

int main()
{
	int i=3,n=1,a[11];
	for(i-=n;i<=10;i++)
	{
		a[i] = i ;
	}
	for(i=2;i<=10;i++)
	{
		printf(" a[%d] => %d \n",i,a[i]) ;
	}
	exit (0);
}
