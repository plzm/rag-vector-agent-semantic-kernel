SELECT [key], JSON_VALUE(metadata, '$.text')
FROM [jh-memory]
WHERE JSON_VALUE(metadata, '$.text') LIKE '%Industry Trends%'


DECLARE @inputText NVARCHAR(MAX) = 'Industry Trends';
DECLARE @retval int, @response NVARCHAR(MAX);
DECLARE @payload NVARCHAR(MAX) = JSON_OBJECT('input': @inputText);
DECLARE @embedding NVARCHAR(MAX)

EXEC @retval = sp_invoke_external_rest_endpoint
    @url = 'https://<resource name>.openai.azure.com/openai/deployments/<deployment name>/embeddings?api-version=2023-03-15-preview',
    @method = 'POST',
    @headers = '{"api-key":"<api-key>"}',
    @payload = @payload,
    @response = @response OUTPUT;

--SELECT @response;

SET @embedding = JSON_QUERY(@response, '$.result.data[0].embedding')

SELECT TOP 5 [key], JSON_VALUE(metadata, '$.text'), VECTOR_DISTANCE('cosine', CAST(@embedding as vector(1536)), embedding) AS similarity
FROM [jh-memory]
ORDER BY similarity ASC