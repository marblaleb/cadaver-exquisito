import 'package:flutter_riverpod/flutter_riverpod.dart';

final wordCountProvider = StateProvider.family<int, String>((ref, text) {
  if (text.trim().isEmpty) return 0;
  return text.trim().split(RegExp(r'\s+')).length;
});
