[CmdletBinding(ConfirmImpact='None')]
param(
   [Parameter(Mandatory=$true,
              Position=0,
              ValueFromPipeline=$true)]
   [Object[]]
   $object
)

PROCESS {
   function ShouldSerializeType($type) {
      $type.IsPrimitive -or `
      $type -eq [Decimal] -or `
      $type -eq [String] -or `
      $type -eq [Array] -or `
      $type -eq [Hashtable]
   }

   function GetDateValueInISO8601($dateTime) {
      $dateTime.ToUniversalTime() | Get-Date -Format 'o'
   }

   foreach ($o in $object) {
      $objectToConvert = New-Object 'System.Management.Automation.PSObject'

      $typeName = $o.GetType().FullName
      $interfaces = $o.GetType().GetInterfaces() | Foreach-Object { $_.FullName }

      $objectToConvert | Add-Member -MemberType NoteProperty -Name 'TypeName' -Value $typeName
      $objectToConvert | Add-Member -MemberType NoteProperty -Name 'Interfaces' -Value $interfaces

      # Special handling for date time values, convert to ISO 8601 format
      if ($o -is [DateTime]) {
         $o = GetDateValueInISO8601 $o

      }

      $objectType = $o.GetType()
      $value = $null

      if (ShouldSerializeType $objectType) {
         $value = $o
      } else {
         $publicProperties = $object | Get-Member -MemberType Properties

         $value = New-Object 'System.Management.Automation.PSObject'

         foreach ($propInfo in $publicProperties) {
            $propValue = $o.$($propInfo.Name)
            if ($propValue -ne $null) {

               # Special handling for date time values, convert to ISO 8601 format
               if ($propValue -is [DateTime]) {
                  $propValue = GetDateValueInISO8601 $propValue
               }

               # Do value conversion for the output Json
               $propValueType = $propValue.GetType()

               # Do not expand nested complex type in the output json
               # Get their string representation instead
               if (-not (ShouldSerializeType $propValueType)) {
                   $propValue = $propValue.ToString()
               }
            }

            # Add Member for the Json presentation
            $value | Add-Member -MemberType NoteProperty -Name $propInfo.Name -Value $propValue
          }
       }

       $objectToConvert | Add-Member -MemberType NoteProperty -Name 'Value' -Value $value

       $objectToConvert | ConvertTo-Json -Depth 1
    }
}