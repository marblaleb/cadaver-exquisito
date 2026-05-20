import 'package:flutter/material.dart';
import 'package:flutter_riverpod/flutter_riverpod.dart';
import 'package:google_fonts/google_fonts.dart';
import '../../../core/api/api_client.dart';
import '../../../core/api/endpoints.dart';
import '../../../core/theme/app_theme.dart';
import '../../editor/providers/word_count_provider.dart';

class CreateCadaverSheet extends ConsumerStatefulWidget {
  const CreateCadaverSheet({super.key});

  @override
  ConsumerState<CreateCadaverSheet> createState() =>
      _CreateCadaverSheetState();
}

class _CreateCadaverSheetState extends ConsumerState<CreateCadaverSheet> {
  final _contentController = TextEditingController();
  int _participants = 3;
  bool _loading = false;

  @override
  void dispose() {
    _contentController.dispose();
    super.dispose();
  }

  Future<void> _submit() async {
    final wordCount =
        ref.read(wordCountProvider(_contentController.text));
    if (wordCount > 300 || wordCount == 0) return;

    setState(() => _loading = true);
    try {
      await apiClient.post(Endpoints.cadavers, data: {
        'maxParticipants': _participants,
        'content': _contentController.text,
      });
      if (mounted) Navigator.pop(context);
    } catch (e) {
      if (mounted) {
        ScaffoldMessenger.of(context)
            .showSnackBar(SnackBar(content: Text('Error: $e')));
      }
    } finally {
      if (mounted) setState(() => _loading = false);
    }
  }

  @override
  Widget build(BuildContext context) {
    final wordCount =
        ref.watch(wordCountProvider(_contentController.text));
    final overLimit = wordCount > 300;

    return Container(
      padding: EdgeInsets.fromLTRB(
          24, 24, 24, MediaQuery.of(context).viewInsets.bottom + 24),
      decoration: const BoxDecoration(
        color: AppColors.background,
        borderRadius: BorderRadius.vertical(top: Radius.circular(28)),
      ),
      child: Column(
        mainAxisSize: MainAxisSize.min,
        crossAxisAlignment: CrossAxisAlignment.start,
        children: [
          Center(
            child: Container(
              width: 40,
              height: 4,
              decoration: BoxDecoration(
                  color: AppColors.textMuted.withValues(alpha: 0.3),
                  borderRadius: BorderRadius.circular(2)),
            ),
          ),
          const SizedBox(height: 20),
          Text(
            'Nueva historia',
            style: GoogleFonts.playfairDisplay(
                fontSize: 22,
                fontWeight: FontWeight.bold,
                color: AppColors.textDark),
          ),
          const SizedBox(height: 20),
          Row(
            children: [
              Text('Participantes:',
                  style: GoogleFonts.dmSans(color: AppColors.textMuted)),
              const SizedBox(width: 16),
              for (final n in [2, 3, 4, 5, 6])
                GestureDetector(
                  onTap: () => setState(() => _participants = n),
                  child: Container(
                    margin: const EdgeInsets.only(right: 8),
                    width: 36,
                    height: 36,
                    decoration: BoxDecoration(
                      color: _participants == n
                          ? AppColors.primary
                          : AppColors.surface,
                      borderRadius: BorderRadius.circular(10),
                    ),
                    child: Center(
                      child: Text(
                        '$n',
                        style: GoogleFonts.dmSans(
                          color: _participants == n
                              ? Colors.white
                              : AppColors.textDark,
                          fontWeight: FontWeight.w600,
                        ),
                      ),
                    ),
                  ),
                ),
            ],
          ),
          const SizedBox(height: 16),
          TextField(
            controller: _contentController,
            onChanged: (_) => setState(() {}),
            maxLines: 6,
            style: GoogleFonts.playfairDisplay(
                color: AppColors.textDark, fontSize: 15),
            decoration: InputDecoration(
              hintText: 'Comienza la historia...',
              hintStyle: GoogleFonts.playfairDisplay(
                  color: AppColors.textMuted),
            ),
          ),
          const SizedBox(height: 8),
          Align(
            alignment: Alignment.centerRight,
            child: Text(
              '$wordCount / 300',
              style: GoogleFonts.dmSans(
                color: overLimit ? Colors.red : AppColors.textMuted,
                fontSize: 12,
              ),
            ),
          ),
          const SizedBox(height: 16),
          SizedBox(
            width: double.infinity,
            child: ElevatedButton(
              onPressed:
                  (_loading || overLimit || wordCount == 0) ? null : _submit,
              child: _loading
                  ? const SizedBox(
                      height: 20,
                      width: 20,
                      child: CircularProgressIndicator(
                          strokeWidth: 2, color: Colors.white))
                  : const Text('Iniciar historia'),
            ),
          ),
        ],
      ),
    );
  }
}
