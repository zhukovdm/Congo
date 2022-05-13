DOXY_DIR := assets/doxygen

.PHONY: all docs clean

all:
	echo "make"

docs:
	doxygen

clean:
	rm -rf $(DOXY_DIR)
