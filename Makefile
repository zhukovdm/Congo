DOXY_DIR := assets/doxygen

.PHONY: all docs clean

all:
	echo "make"

docs:
	doxygen

tests:
	dotnet test

clean:
	rm -rf $(DOXY_DIR)
