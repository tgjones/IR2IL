#include <stdlib.h>
#include <stdio.h>
#include <math.h>

int main()
{
	long int a[10]={
		10,1,9,2,8,3,7,4,6,5	};
	long int b[10],c[10];
	int  i,l,j=0,k=0;
        for(i=0;i<10;i++)
        {
           b[i]=0;
           c[i]=0;
        }

	for(i=0;i<10;i++) 
	{
		if(a[i]>5)
		{
			b[j]=a[i];
			j++;
		}
		else
		{
			c[k]=a[i];
		}
	}
	for(l=0;l<j;l++) 
	{
		printf("%ld \n",b[l]);
	}
	j=0;
	for(i=0;i<10;i++) 
	{
		if(a[i]>5)
		{
			b[i]=a[j];
			j++;
		}
		else
		{
			c[k]=a[i];
		}
	}
	for(l=0;l<j;l++) 
	{
		printf("%ld \n",b[l]);
	}
	exit (0);
}
