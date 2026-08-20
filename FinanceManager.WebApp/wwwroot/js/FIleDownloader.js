window.downloadFile = (fileName, contentType, bytes) => {
    const blob = new Blob([new Uint8Array(bytes)], {
        type: contentType
    });

    const url = URL.createObjectURL(blob);

    const link = document.createElement("a");
    link.href = url;
    link.download = fileName;
    link.click();

    link.remove();
    URL.revokeObjectURL(url);
};