
#include <git2.h>
#include <stdint.h>
#include <stdio.h>

#define PROGRAM_NAME "RAFT - Repo As Filesystem Translator"

#define IS_EC(ret) (ret < 0)

/* Logging macros, for easy switch to file logging */

#define LOG_INFO(message, ...) printf(message, ##__VA_ARGS__)
#define LOG_ERROR(message, ...) fprintf(stderr, message, ##__VA_ARGS__)

int main(int argc, char* argv[])
{
	LOG_INFO("Starting " PROGRAM_NAME);
	int retVal = git_libgit2_init();
	if (IS_EC(retVal))
	{
		LOG_ERROR("Failed libgit2_init, ec=0x%08X", retVal);
		return -1;
	}

exit:
	git_libgit2_shutdown();
	return 0;
}
